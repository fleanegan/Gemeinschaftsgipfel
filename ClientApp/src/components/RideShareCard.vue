<template>
  <li class="card_scroll_container">
    <div :class="{ topic_card_header: true, ride_canceled: isCanceledOrCompleted }">
      <button class="action_button expand-button" @click="$emit('toggle-details')">
        <span class="expand-icon">{{ rideShare.expanded ? '−' : '+' }}</span>
      </button>
      <div class="ride-header-info">
        <span class="ride-route">{{ rideShare.from }} → {{ rideShare.to }}</span>
        <span v-if="showDriver" class="ride-driver">({{ rideShare.driverUserName }})</span>
      </div>
      <span class="seat-badge" :class="seatBadgeClass">{{ rideShare.reservationCount }}/{{ rideShare.availableSeats }}</span>
      <slot name="action-button"></slot>
    </div>
    <div v-if="rideShare.expanded" class="topic-card-details">
      <div class="details-layout">
        <div class="ride-info-container">
          <!-- Route visualization -->
          <div class="route-section">
            <div class="route-visualization">
              <span class="route-point">{{ rideShare.from }}</span>
              <template v-if="parsedStops.length > 0">
                <span v-for="(stop, index) in parsedStops" :key="index" class="route-stop">
                  <span class="route-separator">●</span>
                  <span class="route-point">{{ stop }}</span>
                </span>
              </template>
              <span class="route-separator">●</span>
              <span class="route-point route-destination">{{ rideShare.to }}</span>
            </div>
          </div>

          <!-- Departure time -->
          <div class="info-row">
            <span class="info-label">ABFAHRT</span>
            <span class="info-value">{{ formatDateTime(rideShare.departureTime) }}</span>
          </div>

          <!-- Seats -->
          <div class="info-row">
            <span class="info-label">PLÄTZE</span>
            <span class="info-value">{{ rideShare.reservationCount }}/{{ rideShare.availableSeats }} reserviert</span>
          </div>

          <!-- Driver -->
          <div class="info-row">
            <span class="info-label">FAHRER</span>
            <span class="info-value">{{ rideShare.driverUserName }}</span>
          </div>

          <!-- Passengers -->
          <div v-if="rideShare.passengerUserNames && rideShare.passengerUserNames.length > 0" class="info-row">
            <span class="info-label">MITFAHRER</span>
            <span class="info-value">{{ rideShare.passengerUserNames.join(', ') }}</span>
          </div>

          <!-- Status badge -->
          <div v-if="rideShare.status !== 0" class="info-row">
            <span class="status-badge" :class="statusClass">{{ statusText }}</span>
          </div>

          <!-- Notes section -->
          <div v-if="rideShare.description" class="notes-section">
            <span class="notes-label">NOTIZEN</span>
            <p class="notes-content">{{ rideShare.description }}</p>
          </div>

          <slot name="actions"></slot>

          <!-- Mobile only: Comments button -->
          <button class="mobile-comments-button" @click="openCommentModal">
            <span class="comments-count">{{ rideShare.comments.length }}</span>
            Kommentare anzeigen
          </button>
        </div>

        <!-- Desktop: Comments sidebar -->
        <div class="comments-sidebar">
          <CommentSection
            :comments="rideShare.comments"
            :item-id="rideShare.id"
            api-endpoint="api/rideshare/CommentOnRideShare/"
            @comment-sent="handleCommentSent"
          />
        </div>
      </div>
    </div>

    <!-- Mobile modal for comments -->
    <CommentModal 
      :is-open="isCommentModalOpen"
      :comments="rideShare.comments"
      :item-id="rideShare.id"
      api-endpoint="api/rideshare/CommentOnRideShare/"
      @close="closeCommentModal"
      @comment-sent="handleCommentSent"
    />
  </li>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {RideShare, RideShareStatus} from '@/types/RideShareInterfaces';
import {formatDateTime} from '@/utils/dateFormatter';
import CommentSection from '@/components/CommentSection.vue';
import CommentModal from '@/components/CommentModal.vue';

export default defineComponent({
  components: {
    CommentSection,
    CommentModal
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
  data() {
    return {
      isCommentModalOpen: false
    };
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
    },
    seatBadgeClass(): string {
      const available = this.rideShare.availableSeats - this.rideShare.reservationCount;
      if (available === 0) return 'seat-full';
      if (available <= 1) return 'seat-almost-full';
      return 'seat-available';
    },
    parsedStops(): string[] {
      if (!this.rideShare.stops) return [];
      return this.rideShare.stops
        .split(',')
        .map(stop => stop.trim())
        .filter(stop => stop.length > 0);
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
    openCommentModal() {
      this.isCommentModalOpen = true;
    },
    closeCommentModal() {
      this.isCommentModalOpen = false;
    },
    formatDateTime
  }
});
</script>

<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css"></style>
<style scoped>
/* Card container */
.card_scroll_container {
  background-color: var(--color-nuance-light);
  border-radius: 6px;
  margin-bottom: 0.75rem;
  overflow: hidden;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
}

/* Header section */
.topic_card_header {
  padding: 1rem;
  background-color: var(--color-background);
  border-left: none;
  min-height: auto;
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 0.5rem;
  max-width: 100%;
  box-sizing: border-box;
}

.ride_canceled {
  opacity: 0.6;
  background-color: #f0f0f0;
}

/* Very small screens: Stack expand, action button, and seat badge vertically on the left */
@media (max-width: 400px) {
  .topic_card_header {
    display: grid;
    grid-template-columns: auto 1fr;
    grid-template-rows: auto auto auto;
    gap: 0.5rem;
    align-items: start;
    padding: 0.75rem;
  }
  
  /* Left column: vertical stack of buttons and badge - all same size */
  .expand-button {
    grid-column: 1;
    grid-row: 1;
    width: 2rem;
    height: 2rem;
  }
  
  .ride-header-info {
    grid-column: 2;
    grid-row: 1 / 4;
    margin-left: 0;
    align-self: center;
  }
  
  .seat-badge {
    grid-column: 1;
    grid-row: 2;
    margin-right: 0;
    width: 2rem;
    height: 2rem;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.6875rem;
  }
  
  .topic_card_header :deep(.action_button:not(.expand-button)) {
    grid-column: 1;
    grid-row: 3;
    margin-left: 0;
    width: 2rem;
    height: 2rem;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

/* Expand/collapse button with plus/minus */
.expand-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  padding: 0;
  border-radius: 4px;
  background-color: var(--color-nuance-light);
  transition: background-color 0.2s ease;
}

.expand-button:hover {
  background-color: var(--color-border-light);
}

.expand-icon {
  font-size: 1.5rem;
  font-weight: 300;
  color: var(--color-main-text);
  line-height: 1;
}

/* Header info */
.ride-header-info {
  display: flex;
  flex-direction: column;
  margin-left: 1rem;
  flex: 1;
  min-width: 0;
}

.ride-route {
  font-weight: 600;
  font-size: 1rem;
  color: var(--color-primary);
}

.ride-driver {
  font-size: 0.875rem;
  color: var(--color-main-text);
  margin-top: 0.125rem;
}

/* Seat availability badge */
.seat-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.625rem;
  border-radius: 4px;
  font-size: 0.8125rem;
  font-weight: 600;
  margin-right: 0.5rem;
  white-space: nowrap;
}

.seat-available {
  background-color: #e8f5e9;
  color: #2e7d32;
}

.seat-almost-full {
  background-color: #fff3e0;
  color: #e65100;
}

.seat-full {
  background-color: #ffebee;
  color: #c62828;
}

/* Details section */
.topic-card-details {
  padding: 1rem;
  padding-left: calc(2rem + 1rem + 1rem);
  background-color: var(--color-background);
  border-radius: 0;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  overflow: hidden;
}

.topic-card-details {
    padding: 0.75rem;
    padding-left: 1rem;
    padding-right: 1rem;
  }

/* Layout container for side-by-side on desktop */
.details-layout {
  display: flex;
  flex-direction: column;
  gap: 0;
  width: 100%;
  max-width: 100%;
  overflow: hidden;
}

.ride-info-container {
  display: flex;
  flex-direction: column;
  padding: 0;
  gap: 1rem;
  width: 100%;
  max-width: 100%;
  min-width: 0;
  box-sizing: border-box;
}

/* Mobile: Hide comments sidebar, show button */
.comments-sidebar {
  display: none;
}

.mobile-comments-button {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  margin-top: 1rem;
  padding: 0.875rem;
  background-color: var(--color-nuance-light);
  border: 1px solid var(--color-border-light);
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9375rem;
  font-weight: 500;
  color: var(--color-primary);
  transition: background-color 0.2s ease;
}

.mobile-comments-button:hover {
  background-color: var(--color-border-light);
}

.comments-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.5rem;
  height: 1.5rem;
  padding: 0 0.375rem;
  background-color: var(--color-primary);
  color: var(--color-background);
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
}

/* Desktop: Side-by-side layout */
@media (min-width: 1200px) {
  .details-layout {
    flex-direction: row;
    gap: 0;
    align-items: flex-start;
  }

  .ride-info-container {
    flex: 1 1 45%;
    min-width: 0;
    padding-right: 1rem;
  }

  .comments-sidebar {
    display: block;
    flex: 1 1 55%;
    min-width: 0;
  }

  .mobile-comments-button {
    display: none;
  }
}

/* Route visualization section */
.route-section {
  padding: 0.25rem 0 1rem 0;
  border-bottom: 1px solid var(--color-border-light);
}

.route-visualization {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.375rem;
  line-height: 1.4;
  width: 100%;
}

.route-point {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-primary);
}

.route-destination {
  color: var(--color-primary);
}

.route-separator {
  color: var(--color-main-text);
  opacity: 0.4;
  font-size: 0.5rem;
  display: inline-flex;
  align-items: center;
}

.route-stop {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
}

/* Info rows with muted labels */
.info-row {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-label {
  font-size: 0.6875rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
}

.info-value {
  font-size: 1rem;
  font-weight: 500;
  color: var(--color-primary);
}

/* Notes section */
.notes-section {
  margin-top: 0.5rem;
}

.notes-label {
  display: block;
  font-size: 0.6875rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
  margin-bottom: 0.5rem;
}

.notes-content {
  font-size: 0.9375rem;
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding: 0.75rem;
  border-radius: 4px;
}

/* Status badges */
.status-badge {
  display: inline-block;
  padding: 0.375rem 0.875rem;
  border-radius: 4px;
  font-weight: 600;
  font-size: 0.875rem;
  width: fit-content;
}

.status-canceled {
  background-color: #fff3e0;
  color: #e65100;
}

.status-completed {
  background-color: #e8f5e9;
  color: #2e7d32;
}
</style>
