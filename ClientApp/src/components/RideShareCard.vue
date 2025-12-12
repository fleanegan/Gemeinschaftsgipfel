<template>
  <li class="card_scroll_container">
    <p v-if="isHighlighted" class="most_liked_hint">Meine Reservierung</p>
    <div class="ride-card-positioning-wrapper">
      <div v-if="isCanceledOrCompleted" class="canceled-strike-bar">
        <span class="canceled-text">Abgesagt</span>
      </div>
      <div class="ride-card-wrapper">
      <div :class="{ topic_card_header: true, ride_canceled: isCanceledOrCompleted, most_liked_highlight: isHighlighted }">
        <button class="action_button expand-button" @click="$emit('toggle-details')">
          <span class="expand-icon">{{ rideShare.expanded ? '−' : '+' }}</span>
        </button>
      <div class="ride-header-info">
        <span class="ride-route">{{ rideShare.from }} → {{ rideShare.to }}</span>
        <span v-if="showDriver" class="ride-driver">({{ rideShare.driverUserName }})</span>
      </div>
      <span v-if="!isCanceledOrCompleted" class="seat-badge" :class="seatBadgeClass">{{ rideShare.reservationCount }}/{{ rideShare.availableSeats }}</span>
      <div v-if="!isCanceledOrCompleted" class="header-actions">
        <slot name="action-button"></slot>
      </div>
      </div>
      <div v-if="isCanceledOrCompleted" class="header-actions-overlay">
        <slot name="action-button"></slot>
      </div>
      </div>
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
            <span class="info-label">ES FÄHRT</span>
            <span class="info-value">{{ rideShare.driverUserName }}</span>
          </div>

          <!-- Passengers -->
          <div v-if="rideShare.passengerUserNames && rideShare.passengerUserNames.length > 0" class="info-row">
            <span class="info-label">ES FAHREN MIT</span>
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

          <!-- Mobile only: Comments button -->
          <button class="mobile-comments-button" @click="openCommentModal">
            <span class="comments-count">{{ rideShare.comments?.length || 0 }}</span>
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
    },
    isHighlighted: {
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
      return this.rideShare.status === 1 || this.rideShare.status === 2 || 
             this.rideShare.status === 'Canceled' || this.rideShare.status === 'Completed';
    },
    statusClass(): string {
      if (this.rideShare.status === 1 || this.rideShare.status === 'Canceled') return 'status-canceled';
      if (this.rideShare.status === 2 || this.rideShare.status === 'Completed') return 'status-completed';
      return '';
    },
    statusText(): string {
      if (this.rideShare.status === 1 || this.rideShare.status === 'Canceled') return 'Abgesagt';
      if (this.rideShare.status === 2 || this.rideShare.status === 'Completed') return 'Abgeschlossen';
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
  border-radius: var(--radius-sharp);
  margin-bottom: var(--space-sm);
  overflow: visible;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}

.most_liked_hint {
  margin-left: var(--space-lg);
  margin-top: var(--space-sm);
  background-color: var(--color-secondary);
  border-top-left-radius: var(--radius-interactive);
  border-top-right-radius: var(--radius-interactive);
  width: 12rem;
  height: 2rem;
  text-align: center;
  color: var(--color-background);
}

/* Positioning wrapper for cancel bar */
.ride-card-positioning-wrapper {
  position: relative;
  width: 100%;
}

/* Wrapper for card content */
.ride-card-wrapper {
  position: relative;
  width: 100%;
  border-radius: var(--radius-sharp);
  overflow: hidden;
  background-color: var(--color-background);
}

/* Strike-through bar for canceled rides */
.canceled-strike-bar {
  position: absolute;
  top: 50%;
  left: -10px;
  right: -10px;
  height: 6px;
  background-color: var(--color-primary);
  transform: translateY(-50%);
  z-index: 50;
  border-radius: 3px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.4);
  pointer-events: none;
  display: flex;
  align-items: center;
  justify-content: center;
}

.canceled-text {
  background-color: var(--color-primary);
  color: var(--color-background);
  padding: var(--space-xs) var(--space-md);
  font-weight: var(--font-weight-bold);
  font-size: var(--text-sm);
  border-radius: var(--radius-interactive);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.4);
}

/* Header section */
.topic_card_header {
  padding: var(--space-md);
  background-color: var(--color-background);
  border-left: none;
  min-height: auto;
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-sm);
  max-width: 100%;
  box-sizing: border-box;
  position: relative;
  z-index: 1;
}

/*do not delete*/
.most_liked_highlight {
  border-radius: var(--radius-sharp);
  border: .25rem solid var(--color-secondary);
}

/* Header actions container (edit/delete/reserve buttons) */
.header-actions {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-xs);
  position: relative;
  z-index: 100;
}

/* Overlay actions for canceled rides - positioned above the strike bar */
.header-actions-overlay {
  position: absolute;
  top: 50%;
  right: var(--space-md);
  transform: translateY(-50%);
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-xs);
  z-index: 100;
}

.header-actions :deep(.action_button),
.header-actions-overlay :deep(.action_button) {
  position: relative;
  z-index: 100;
  background-color: var(--color-background);
  padding: var(--space-xs);
  border-radius: var(--radius-interactive);
}

.header-actions-overlay :deep(.action_button) {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.15);
}

.ride_canceled {
  opacity: 0.6;
  background-color: #f0f0f0;
}

/* Expand/collapse button with plus/minus */
.expand-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  padding: 0;
  border-radius: var(--radius-interactive);
  background-color: var(--color-nuance-light);
  transition: background-color 0.2s ease;
}

.expand-button:hover {
  background-color: var(--color-border-light);
}

.expand-icon {
  font-size: var(--text-lg);
  font-weight: 300;
  color: var(--color-main-text);
  line-height: 1;
}

/* Header info */
.ride-header-info {
  display: flex;
  flex-direction: column;
  margin-left: var(--space-md);
  flex: 1;
  min-width: 0;
}

.ride-route {
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  color: var(--color-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ride-driver {
  font-size: var(--text-sm);
  color: var(--color-main-text);
  margin-top: var(--space-xs);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Seat availability badge */
.seat-badge {
  display: inline-flex;
  align-items: center;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-interactive);
  font-size: var(--text-sm);
  font-weight: var(--font-weight-semibold);
  margin-right: var(--space-sm);
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
  padding: var(--space-md);
  padding-left: calc(2rem + var(--space-md) + var(--space-md));
  background-color: var(--color-background);
  border-radius: 0;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  overflow: hidden;
}

.topic-card-details {
    padding: var(--space-sm);
    padding-left: var(--space-md);
    padding-right: var(--space-md);
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
  gap: var(--space-md);
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
  gap: var(--space-sm);
  width: 100%;
  margin-top: var(--space-md);
  padding: var(--space-sm);
  background-color: var(--color-nuance-light);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-interactive);
  cursor: pointer;
  font-size: var(--text-base);
  font-weight: var(--font-weight-medium);
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
  min-width: var(--space-lg);
  height: var(--space-lg);
  padding: 0 var(--space-xs);
  background-color: var(--color-primary);
  color: var(--color-background);
  border-radius: var(--radius-pill);
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
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
    padding-right: var(--space-md);
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
  padding: var(--space-xs) 0 var(--space-md) 0;
  border-bottom: 1px solid var(--color-border-light);
}

.route-visualization {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-xs);
  line-height: 1.4;
  width: 100%;
}

.route-point {
  font-size: var(--text-sm);
  font-weight: var(--font-weight-medium);
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
  gap: var(--space-xs);
}

/* Info rows with muted labels */
.info-row {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.info-label {
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
}

.info-value {
  font-size: var(--text-base);
  font-weight: var(--font-weight-medium);
  color: var(--color-primary);
}

/* Notes section */
.notes-section {
  margin-top: var(--space-sm);
}

.notes-label {
  display: block;
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
  margin-bottom: var(--space-sm);
}

.notes-content {
  font-size: var(--text-base);
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding: var(--space-sm);
  border-radius: var(--radius-interactive);
}

/* Status badges */
.status-badge {
  display: inline-block;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-interactive);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-sm);
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

/* Mobile: Reduce whitespace */
@media (max-width: 600px) {
  .topic_card_header {
    padding: var(--space-sm);
    gap: var(--space-xs);
  }
  
  .header-actions {
    gap: 2px;
  }
  
  .header-actions :deep(.action_button) {
    padding: var(--space-xs);
  }
  
  .ride-header-info {
    margin-left: var(--space-sm);
  }
  
  .seat-badge {
    margin-right: var(--space-xs);
    padding: var(--space-xs);
    font-size: var(--text-xs);
  }
}
</style>
