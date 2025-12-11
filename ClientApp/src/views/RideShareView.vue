<template>
  <div class="rideshare wide-content">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Fahrgemeinschaften</h1>
    <div class="instructions-formats-row">
      <div class="instructions-wrapper">
        <InstructionCards :instructions="instructions" />
      </div>
    </div>
    
    <div class="sections-container">
      <!-- Left section: Meine Fahrten -->
      <section class="ride-section">
        <h2>Meine Fahrten</h2>
        <ul class="list">
          <RideShareCard
              v-for="(item, index) in myRideShares"
              :key="item.id"
              :ride-share="item"
              @toggle-details="toggleDetails(myRideShares, index)"
              @comment-sent="handleCommentSent"
          >
            <template #action-button>
              <button v-if="item.status !== 'Canceled' && item.status !== 1" class="action_button" @click="openRemovalModal(item.id)">
                <img alt="Verwalten" src="/trash_bin.svg" title="Verwalten">
              </button>
              <button v-if="item.status === 'Canceled' || item.status === 1" class="action_button" @click="uncancelRideShare(item.id)">
                <img alt="Uncancel" src="/empty_edit_no_border.svg" title="Reaktivieren">
              </button>
              <button class="action_button" @click="editRideShare(item.id)">
                <img alt="Edit" src="/empty_edit_no_border.svg">
              </button>
            </template>
          </RideShareCard>
        </ul>
        <hr>
        <div id="owner_action" class="my-topics-add-button-container">
          <button class="submit-button" @click="addNewRideShare">Fahrt anbieten?</button>
        </div>
      </section>
      
      <!-- Right section: Verfügbare Fahrten -->
      <section class="ride-section">
        <h2>Verfügbare Fahrten</h2>
        <ul class="list">
          <RideShareCard
              v-for="(item, index) in foreignRideShares"
              :key="item.id"
              :ride-share="item"
              :show-driver="true"
              @toggle-details="toggleDetails(foreignRideShares, index)"
              @comment-sent="handleCommentSent"
          >
            <template #action-button>
              <button 
                class="action_button" 
                @click="toggleReservation(index)"
                :disabled="!item.didIReserve && item.availableSeats <= item.reservationCount"
              >
                <img :src="item.didIReserve ? '/full_heart.svg' : '/empty_heart.svg'" alt="Reserve">
              </button>
            </template>
          </RideShareCard>
          <li class="list-end-message">Ende der Liste. Danke fürs Mitfahren!</li>
        </ul>
      </section>
    </div>

    <RideShareRemovalModal
      :is-open="removalModalOpen"
      title="Fahrt verwalten"
      message="Möchtest du die Fahrt absagen (Mitfahrer werden benachrichtigt) oder komplett löschen?"
      @cancel-ride="handleCancelRide"
      @delete-ride="handleDeleteRide"
      @cancel="closeRemovalModal"
    />
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import type {RideShare} from "@/types/RideShareInterfaces";
import type {Comment} from "@/types/TopicInterfaces";
import RideShareCard from "@/components/RideShareCard.vue";
import RideShareRemovalModal from '@/components/RideShareRemovalModal.vue';
import {scrollToTopMixin} from '@/mixins/scrollToTop';
import InstructionCards from '@/components/InstructionCards.vue';
import {rideShareService} from '@/services/api';

export default defineComponent({
  components: {RideShareCard, RideShareRemovalModal, InstructionCards},
  mixins: [scrollToTopMixin],
  data() {
    return {
      foreignRideShares: [] as RideShare[],
      myRideShares: [] as RideShare[],
      removalModalOpen: false,
      rideShareToManage: null as string | null,
      instructions: [
        {
          title: 'Fahrt anbieten',
          content: 'Biete eine Mitfahrgelegenheit an und teile deine Route mit anderen. Gib an, wie viele Plätze verfügbar sind und wann du losfährst.'
        },
        {
          title: 'Platz reservieren',
          content: 'Finde passende Fahrten und reserviere einen Platz. Die Anzahl verfügbarer Plätze wird live aktualisiert.'
        },
        {
          title: 'Gemeinsam fahren',
          content: 'Kommuniziere mit Fahrer und Mitfahrern über die Kommentarfunktion. Spare Kosten und schone die Umwelt!'
        }
      ]
    };
  },
  methods: {
    async fetchData() {
      this.myRideShares = await rideShareService.getMyRideShares();
      this.foreignRideShares = await rideShareService.getForeignRideShares();
      
      // Sort by departure time (upcoming first)
      const sortByDepartureTime = (a: RideShare, b: RideShare) => {
        return new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime();
      };
      
      this.myRideShares.sort(sortByDepartureTime);
      this.foreignRideShares.sort(sortByDepartureTime);
    },
    async toggleDetails(rideShares: RideShare[], index: number): Promise<void> {
      const item = rideShares[index];
      if (!item) return;
      if (item.isLoading) return;
      try {
        item.expanded = !item.expanded;
        if (item.expanded) {
          item.isLoading = true;
          item.comments = await rideShareService.getComments(item.id);

          item.comments.sort((a: Comment, b: Comment) => {
            return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
          });
        } else {
          item.comments = [];
        }
      } catch (error) {
        console.error('Error toggling details:', error);
        // Revert expanded state on error
        item.expanded = !item.expanded;
      } finally {
        // Clear loading state
        item.isLoading = false;
      }
    },
    async toggleReservation(index: number): Promise<void> {
      const rideShare = this.foreignRideShares[index];
      if (!rideShare) return;
      
      // Check if there are available seats before allowing reservation
      if (!rideShare.didIReserve && rideShare.availableSeats <= rideShare.reservationCount) {
        return;
      }
      
      try {
        if (!rideShare.didIReserve) {
          await rideShareService.addReservation(rideShare.id);
          rideShare.reservationCount++;
        } else {
          await rideShareService.removeReservation(rideShare.id);
          rideShare.reservationCount--;
        }
        rideShare.didIReserve = !rideShare.didIReserve;
        
        // Refresh to get updated passenger list
        await this.fetchData();
      } catch (error) {
        console.error('Error toggling reservation:', error);
      }
    },
    editRideShare(rideShareId: string): void {
      this.$router.push({
        name: 'Fahrt bearbeiten',
        params: {
          'rideShareId': rideShareId,
        }
      });
    },
    openRemovalModal(rideShareId: string) {
      this.rideShareToManage = rideShareId;
      this.removalModalOpen = true;
    },
    closeRemovalModal() {
      this.removalModalOpen = false;
      this.rideShareToManage = null;
    },
    async handleCancelRide() {
      if (!this.rideShareToManage) return;
      
      try {
        await rideShareService.cancelRideShare(this.rideShareToManage);
        await this.fetchData();
        this.closeRemovalModal();
      } catch (error) {
        console.error('Error canceling ride share:', error);
      }
    },
    async handleDeleteRide() {
      if (!this.rideShareToManage) return;
      
      try {
        await rideShareService.deleteRideShare(this.rideShareToManage);
        await this.fetchData();
        this.closeRemovalModal();
      } catch (error) {
        console.error('Error deleting ride share:', error);
      }
    },
    async uncancelRideShare(rideShareId: string) {
      await rideShareService.uncancelRideShare(rideShareId);
      await this.fetchData();
    },
    addNewRideShare() {
      this.$router.push("/rideshare/add");
    },
    async handleCommentSent(payload: { rideShareId: string, comment: Comment, content: string }): Promise<void> {
      const updateRideShareComments = (rideShares: RideShare[]) => {
        const rideShareIndex = rideShares.findIndex(r => r.id === payload.rideShareId);
        const rideShare = rideShares[rideShareIndex];
        if (rideShareIndex !== -1 && rideShare && rideShare.expanded) {
          if (payload.comment) {
            rideShare.comments.unshift(payload.comment);
          } else {
            this.refreshRideShareComments(rideShare);
          }
        }
      };

      updateRideShareComments(this.myRideShares);
      updateRideShareComments(this.foreignRideShares);
    },
    async refreshRideShareComments(rideShare: RideShare): Promise<void> {
      if (rideShare.expanded) {
        rideShare.comments = await rideShareService.getComments(rideShare.id);
        rideShare.comments.sort((a: Comment, b: Comment) => {
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
      }
    },
  },
  mounted() {
    this.fetchData();
  },
});
</script>

<style scoped>
.rideshare {
  background-color: var(--color-background);
}

/* Remove left margin from headings to align with content */
.rideshare h1 {
  margin-left: 0;
}

.rideshare h2 {
  margin-left: 0;
}

/* Constrain instruction cards width */
.instructions-wrapper {
  max-width: 600px;
  margin-left: -0.25rem;
}

/* Instructions row (single column for RideShareView) */
.instructions-formats-row {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  margin-bottom: var(--space-xl);
  align-items: center;
}

/* Sections container - always stacked vertically */
.sections-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl);
  max-width: 100%;
  overflow-x: hidden;
}

.ride-section {
  flex: 1;
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
}

/* Desktop: Keep sections stacked */
@media (min-width: 1200px) {
  .sections-container {
    flex-direction: column;
    gap: var(--space-xl);
  }
}

.list {
  background-color: var(--color-background-soft);
  padding: var(--space-md);
  border-radius: var(--radius-sharp);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  list-style: none;
  margin: 0;
}

/* Small screens: Reduce list padding */
@media (max-width: 400px) {
  .list {
    padding: var(--space-sm) var(--space-sm) !important;
    gap: var(--space-xs) !important;
  }
}

.list-end-message {
  text-align: center;
  padding: var(--space-md);
  color: var(--color-main-text);
  opacity: 0.7;
  font-size: var(--text-sm);
}

/* Action button styling */
.action_button {
  cursor: pointer;
  background: none;
  border-style: none;
  border-radius: var(--radius-interactive);
  font-weight: bold;
  display: flex;
  place-items: center;
}

.topic_card_details_owner_actions {
  display: flex;
  flex-direction: row;
  align-content: center;
  align-items: center;
  gap: var(--space-sm);
}

.topic_card_details_owner_actions button {
  margin-left: var(--space-sm);
}

.my-topics-add-button-container {
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
}

.submit-button {
  margin-top: 0;
  margin-left: auto;
  border-top-left-radius: 0;
  border-top-right-radius: 0;
  border-bottom-left-radius: var(--radius-interactive);
  border-bottom-right-radius: var(--radius-interactive);
  margin-right: 0;
  white-space: nowrap;
  padding-left: var(--space-lg);
  padding-right: var(--space-lg);
  width: auto;
}

.floating_scroll_to_top_hidden {
  margin-left: auto;
  height: 3.3rem;
  width: 75%;
  position: sticky;
  top: 0;
  left: 0;
  z-index: 9999;
  display: none;
}

.floating_scroll_to_top_shown {
  display: flex;
  flex-direction: row;
  align-content: center;
  justify-content: right;
  background-color: transparent;
}

@media (max-width: 900px) {
  .floating_scroll_to_top_shown {
    background-color: var(--color-background);
  }
}
</style>
<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css"></style>
