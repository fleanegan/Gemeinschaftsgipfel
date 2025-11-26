<template>
  <li class="card_scroll_container">
    <div :class="{ topic_card_header: true, ride_canceled: isCanceledOrCompleted }">
      <button class="action_button" @click="$emit('toggle-details')">
        <img :src="rideShare.expanded ? 'collapse.svg' : '/expand.svg'" alt="Expand">
      </button>
      <div class="ride-header-info">
        <span class="ride-route">{{ rideShare.from }} → {{ rideShare.to }}</span>
        <span v-if="showDriver" class="ride-driver">({{ rideShare.driverUserName }})</span>
      </div>
      <slot name="action-button"></slot>
    </div>
    <div v-if="rideShare.expanded" class="topic-card-details">
      <div class="ride-info-container">
        <div class="ride-info">
          <p class="ride-label">Von:</p>
          <p class="ride-value">{{ rideShare.from }}</p>
        </div>
        <div class="ride-info">
          <p class="ride-label">Nach:</p>
          <p class="ride-value">{{ rideShare.to }}</p>
        </div>
        <div class="ride-info">
          <p class="ride-label">Abfahrt:</p>
          <p class="ride-value">{{ formatDateTime(rideShare.departureTime) }}</p>
        </div>
        <div class="ride-info">
          <p class="ride-label">Plätze:</p>
          <p class="ride-value">{{ rideShare.reservationCount }}/{{ rideShare.availableSeats }} reserviert</p>
        </div>
        <div v-if="rideShare.stops" class="ride-info">
          <p class="ride-label">Zwischenstopps:</p>
          <p class="ride-value">{{ rideShare.stops }}</p>
        </div>
        <div v-if="rideShare.description" class="ride-info">
          <p class="ride-label">Beschreibung:</p>
          <p class="ride-value description">{{ rideShare.description }}</p>
        </div>
        <div class="ride-info">
          <p class="ride-label">Fahrer:</p>
          <p class="ride-value">{{ rideShare.driverUserName }}</p>
        </div>
        <div v-if="rideShare.passengerUserNames && rideShare.passengerUserNames.length > 0" class="ride-info">
          <p class="ride-label">Mitfahrer:</p>
          <p class="ride-value">{{ rideShare.passengerUserNames.join(', ') }}</p>
        </div>
        <div v-if="rideShare.status !== 0" class="ride-info">
          <span class="status-badge" :class="statusClass">{{ statusText }}</span>
        </div>
        <slot name="actions"></slot>
      </div>
      <CommentSection
        :comments="rideShare.comments"
        :item-id="rideShare.id"
        api-endpoint="api/rideshare/CommentOnRideShare/"
        @comment-sent="handleCommentSent"
      />
    </div>
  </li>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {RideShare, RideShareStatus} from '@/types/RideShareInterfaces';
import {formatDateTime} from '@/utils/dateFormatter';
import CommentSection from '@/components/CommentSection.vue';

export default defineComponent({
  components: {
    CommentSection
  },
  props: {
    rideShare: {
      type: Object as PropType<RideShare>,
      required: true
    },
    showDriver: {
      type: Boolean,
      default: false
    }
  },
  emits: ['toggle-details', 'comment-sent'],
  computed: {
    isCanceledOrCompleted(): boolean {
      return this.rideShare.status === 1 || this.rideShare.status === 2; // Canceled or Completed
    },
    statusClass(): string {
      if (this.rideShare.status === 1) return 'status-canceled';
      if (this.rideShare.status === 2) return 'status-completed';
      return '';
    },
    statusText(): string {
      if (this.rideShare.status === 1) return 'Abgesagt';
      if (this.rideShare.status === 2) return 'Abgeschlossen';
      return '';
    }
  },
  methods: {
    handleCommentSent(payload: any) {
      this.$emit('comment-sent', {
        rideShareId: payload.itemId,
        comment: payload.comment,
        content: payload.content
      });
    },
    formatDateTime
  }
});
</script>

<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css"></style>
<style scoped>
.ride-info-container {
  display: flex;
  flex-direction: column;
  padding: 0.5rem 1rem;
}

.ride-info {
  display: flex;
  flex-direction: row;
  margin-bottom: 0.5rem;
  align-items: flex-start;
}

.ride-label {
  font-weight: bold;
  min-width: 130px;
  margin-right: 1rem;
}

.ride-value {
  flex: 1;
}

.ride-value.description {
  white-space: pre-wrap;
}

.ride_canceled {
  opacity: 0.6;
  background-color: #f0f0f0;
}

.status-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 0.25rem;
  font-weight: bold;
  font-size: 0.875rem;
}

.status-canceled {
  background-color: #ffc107;
  color: #000;
}

.status-completed {
  background-color: #28a745;
  color: #fff;
}

.ride-header-info {
  display: flex;
  flex-direction: column;
  margin-left: 1rem;
  flex: 1;
}

.ride-route {
  font-weight: bold;
  font-size: 1.1rem;
}

.ride-driver {
  font-size: 0.9rem;
  color: #666;
  margin-top: 0.25rem;
}
</style>
