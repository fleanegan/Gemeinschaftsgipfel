<template>
  <li class="card_scroll_container">
    <p v-if="isHighlighted" class="most_liked_hint">Publikumsliebling</p>
    <div :class="{ topic_card_header: true, most_liked_highlight: isHighlighted }">
      <button class="action_button expand-button" @click="$emit('toggle-details')">
        <span class="expand-icon">{{ topic.expanded ? '−' : '+' }}</span>
      </button>
      <h4 class="header-title">{{ topic.title }}</h4>
      <div class="header-actions">
        <slot name="action-button"></slot>
        <slot name="vote-button"></slot>
      </div>
    </div>
    <div v-if="topic.expanded" class="topic-card-details">
      <div class="details-layout">
        <div class="topic-info-container">
          <!-- Badges row -->
          <div class="details-badges-row">
            <span class="category-badge" :class="categoryBadgeClass">{{ topic.category }}</span>
            <span class="time-badge">{{ formatDuration(topic.presentationTimeInMinutes) }}</span>
          </div>

          <!-- Presenter (foreign topics only) -->
          <slot name="presenter"></slot>

          <!-- Material (if non-empty) -->
          <div v-if="topic.material" class="info-row">
            <span class="info-label">MATERIAL</span>
            <span class="info-value">{{ topic.material }}</span>
          </div>

          <!-- Description/Notes -->
          <div v-if="topic.description" class="notes-section">
            <span class="notes-label">BESCHREIBUNG</span>
            <p class="notes-content">{{ topic.description }}</p>
          </div>

          <!-- Mobile only: Comments button -->
          <button class="mobile-comments-button" @click="openCommentModal">
            <span class="comments-count">{{ topic.comments.length }}</span>
            Kommentare anzeigen
          </button>
        </div>

        <!-- Desktop: Comments sidebar -->
        <div class="comments-sidebar">
          <CommentSection
            :comments="topic.comments"
            :item-id="topic.id"
            api-endpoint="api/topic/CommentOnTopic/"
            @comment-sent="handleCommentSent"
          />
        </div>
      </div>
    </div>

    <!-- Mobile modal for comments -->
    <CommentModal 
      :is-open="isCommentModalOpen"
      :comments="topic.comments"
      :item-id="topic.id"
      api-endpoint="api/topic/CommentOnTopic/"
      @close="closeCommentModal"
      @comment-sent="handleCommentSent"
    />
  </li>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {MyTopic, ForeignTopic} from '@/types/TopicInterfaces';
import CommentSection from '@/components/CommentSection.vue';
import CommentModal from '@/components/CommentModal.vue';

export default defineComponent({
  components: {
    CommentSection,
    CommentModal
  },
  props: {
    topic: {
      type: Object as PropType<MyTopic | ForeignTopic>,
      required: true
    },
    isHighlighted: {
      type: Boolean,
      default: false
    }
  },
  emits: ['toggle-details', 'comment-sent'],
  data() {
    return {
      isCommentModalOpen: false
    };
  },
  computed: {
    categoryBadgeClass(): string {
      const category = this.topic.category || 'Sonstiges';
      return `category-${category.toLowerCase()}`;
    }
  },
  methods: {
    formatDuration(minutes: number): string {
      if (minutes === -1) {
        return '∞';
      }
      return `${minutes}'`;
    },
    handleCommentSent(payload: any) {
      this.$emit('comment-sent', {
        topicId: payload.itemId,
        comment: payload.comment,
        content: payload.content
      });
    },
    openCommentModal() {
      this.isCommentModalOpen = true;
    },
    closeCommentModal() {
      this.isCommentModalOpen = false;
    }
  }
});
</script>

<style scoped>
/* Card container */
.card_scroll_container {
  background-color: var(--color-nuance-light);
  border-radius: var(--radius-sharp);
  margin-bottom: var(--space-sm);
  overflow: hidden;
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
  color: #2c3e50;
  font-weight: 200;
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
}

/*do not delete*/
.most_liked_highlight {
  border-radius: var(--radius-sharp);
  border: .25rem solid var(--color-secondary);
}

/* Header title */
.header-title {
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  color: var(--color-primary);
  margin: 0;
  padding: 0;
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
}

/* Header actions container (edit/delete/vote buttons) */
.header-actions {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-xs);
}

/* Details badges row */
.details-badges-row {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
  margin-bottom: var(--space-sm);
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
  cursor: pointer;
  border: none;
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

/* Category badges - pill shaped */
.category-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-pill);
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  white-space: nowrap;
  line-height: 1.2;
  /* Default fallback colors */
  background-color: var(--category-sonstiges-bg, #f5f5f5);
  color: var(--category-sonstiges-text, #616161);
}

.category-badge.category-workshop {
  background-color: var(--category-workshop-bg, #e3f2fd);
  color: var(--category-workshop-text, #1565c0);
}

.category-badge.category-vortrag {
  background-color: var(--category-vortrag-bg, #e8f5e9);
  color: var(--category-vortrag-text, #2e7d32);
}

.category-badge.category-sport {
  background-color: var(--category-sport-bg, #fff3e0);
  color: var(--category-sport-text, #e65100);
}

.category-badge.category-diskussion {
  background-color: var(--category-diskussion-bg, #f3e5f5);
  color: var(--category-diskussion-text, #7b1fa2);
}

.category-badge.category-sonstiges {
  background-color: var(--category-sonstiges-bg, #f5f5f5);
  color: var(--category-sonstiges-text, #616161);
}

/* Time badge */
.time-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-pill);
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  background-color: var(--color-nuance-light);
  color: var(--color-main-text);
  white-space: nowrap;
  line-height: 1.2;
}

/* Details section */
.topic-card-details {
  padding: var(--space-md);
  background-color: var(--color-background);
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
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

.topic-info-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  width: 100%;
  min-width: 0;
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
  font-size: var(--text-sm);
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding-right: 0;
  border-radius: var(--radius-interactive);
  margin: 0;
}

/* Notes/Description section */
.notes-section {
  margin-top: var(--space-sm);
}

.notes-label {
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
}

.notes-content {
  font-size: var(--text-sm);
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding-right: 0;
  border-radius: var(--radius-interactive);
  margin: 0;
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
  font-size: var(--text-sm);
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

/* Desktop: Side-by-side layout */
@media (min-width: 785px) {
  .details-layout {
    flex-direction: row;
    gap: var(--space-md);
    align-items: flex-start;
  }

  .topic-info-container {
    flex: 1 1 50%;
    min-width: 0;
  }

  .comments-sidebar {
    display: block;
    flex: 1 1 50%;
    min-width: 0;
  }

  .notes-content {
  }

  .mobile-comments-button {
    display: none;
  }
}
</style>

<!-- Unscoped styles for category badge colors to work with CSS variables -->
<style>
.category-badge.category-workshop {
  background-color: #e3f2fd !important;
  color: #1565c0 !important;
}

.category-badge.category-vortrag {
  background-color: #e8f5e9 !important;
  color: #2e7d32 !important;
}

.category-badge.category-sport {
  background-color: #fff3e0 !important;
  color: #e65100 !important;
}

.category-badge.category-diskussion {
  background-color: #f3e5f5 !important;
  color: #7b1fa2 !important;
}

.category-badge.category-sonstiges {
  background-color: #f5f5f5 !important;
  color: #616161 !important;
}
</style>
