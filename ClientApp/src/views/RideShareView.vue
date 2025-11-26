<template>
  <div class="topic">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Fahrgemeinschaften</h1>
    <div class="instruction_container">
      <div class="instruction_card">
        <div class="instruction_card_content">
          <p class="instruction_card_content_header_title" style="padding-left: 2.25rem">Fahrt anbieten</p>
          <p>Biete eine Mitfahrgelegenheit an und teile deine Route mit anderen. Gib an, wie viele Plätze verfügbar sind und wann du losfährst.</p>
        </div>
        <div class="instruction_card_enumerator">1.</div>
      </div>
      <div class="instruction_card">
        <div class="instruction_card_content">
          <p class="instruction_card_content_header_title" style="padding-left: 2.25rem">Platz reservieren</p>
          <p>Finde passende Fahrten und reserviere einen Platz. Die Anzahl verfügbarer Plätze wird live aktualisiert.</p>
        </div>
        <div class="instruction_card_enumerator">2.</div>
      </div>
      <div class="instruction_card">
        <div class="instruction_card_content">
          <p class="instruction_card_content_header_title" style="padding-left: 2.25rem">Gemeinsam fahren</p>
          <p>Kommuniziere mit Fahrer und Mitfahrern über die Kommentarfunktion. Spare Kosten und schone die Umwelt!</p>
        </div>
        <div class="instruction_card_enumerator">3.</div>
      </div>
    </div>
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
      <li>
        <hr>
        <div id="owner_action" class="my-topics-add-button-container">
          <button class="submit-button" @click="addNewRideShare">Neue Fahrt anbieten?</button>
        </div>
      </li>
    </ul>
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
import axios from "axios";
import type {RideShare} from "@/types/RideShareInterfaces";
import type {Comment} from "@/types/TopicInterfaces";
import RideShareCard from "@/components/RideShareCard.vue";
import {scrollToTopMixin} from '@/mixins/scrollToTop';

export default defineComponent({
  components: {RideShareCard},
  mixins: [scrollToTopMixin],
  data() {
    return {
      foreignRideShares: [] as RideShare[],
      myRideShares: [] as RideShare[],
    };
  },
  methods: {
    async fetchData() {
      this.myRideShares = (await axios.get('/api/rideshare/allOfLoggedIn', {})).data;
      this.foreignRideShares = (await axios.get('/api/rideshare/allExceptLoggedIn', {})).data;
      
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
          const response = await axios.get('/api/rideshare/comments?RideShareId=' + item.id);
          item.comments = response.data;

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
          await axios.post('/api/rideshare/addReservation', {"RideShareId": rideShare.id});
          rideShare.reservationCount++;
        } else {
          await axios.delete('/api/rideshare/removeReservation/' + rideShare.id);
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
      await axios.delete('api/rideshare/delete/' + rideShareId);
      await this.fetchData();
    },
    async cancelRideShare(rideShareId: string) {
      await axios.post('api/rideshare/cancel/' + rideShareId);
      await this.fetchData();
    },
    async uncancelRideShare(rideShareId: string) {
      await axios.post('api/rideshare/uncancel/' + rideShareId);
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
        rideShare.comments = (await axios.get('/api/rideshare/comments?RideShareId=' + rideShare.id)).data;
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
