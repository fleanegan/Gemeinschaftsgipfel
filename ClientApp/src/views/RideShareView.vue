<template>
  <div class="topic">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Fahrgemeinschaften</h1>
    <InstructionCards :instructions="instructions" />
    <h2>Meine Fahrten</h2>
    <ul class="list">
      <RideShareCard
          v-for="(item, index) in myRideShares"
          :key="item.id"
          :ride-share="item"
          @toggle-details="toggleDetails(myRideShares, index)"
          @comment-sent="handleCommentSent"
      >
        <template #actions>
          <div class="topic_card_details_owner_actions">
            <button class="action_button" @click="removeRideShare(item.id)">
              <img alt="Delete" src="/empty_delete.svg">
            </button>
            <button v-if="item.status !== 1" class="action_button" @click="cancelRideShare(item.id)">
              <img alt="Cancel" src="/empty_delete.svg" title="Absagen">
            </button>
            <button v-if="item.status === 1" class="action_button" @click="uncancelRideShare(item.id)">
              <img alt="Uncancel" src="/empty_edit_no_border.svg" title="Reaktivieren">
            </button>
            <button class="action_button" @click="editRideShare(item.id)">
              <img alt="Edit" src="/empty_edit_no_border.svg">
            </button>
          </div>
        </template>
      </RideShareCard>
    </ul>
    <hr>
    <div id="owner_action" class="my-topics-add-button-container">
      <button class="submit-button" @click="addNewRideShare">Fahrt anbieten?</button>
    </div>
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
      <li class="topic-card-details">Ende der Liste. Danke fürs Mitfahren!
        <br style="margin-bottom: 25rem">
      </li>
    </ul>
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import type {RideShare} from "@/types/RideShareInterfaces";
import type {Comment} from "@/types/TopicInterfaces";
import RideShareCard from "@/components/RideShareCard.vue";
import {scrollToTopMixin} from '@/mixins/scrollToTop';
import InstructionCards from '@/components/InstructionCards.vue';
import {rideShareService} from '@/services/api';

export default defineComponent({
  components: {RideShareCard, InstructionCards},
  mixins: [scrollToTopMixin],
  data() {
    return {
      foreignRideShares: [] as RideShare[],
      myRideShares: [] as RideShare[],
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
    async removeRideShare(rideShareId: string) {
      await rideShareService.deleteRideShare(rideShareId);
      await this.fetchData();
    },
    async cancelRideShare(rideShareId: string) {
      await rideShareService.cancelRideShare(rideShareId);
      await this.fetchData();
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
.topic {
  background-color: var(--color-background);
}

.list {
  background-color: var(--color-background-soft);
  padding: 1rem;
  border-radius: 6px;
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
