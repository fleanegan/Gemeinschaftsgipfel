<template>
  <li class="card_scroll_container">
    <div class="forum_card_header">
      <button class="action_button expand-button" @click="$emit('toggle-details')">
        <span class="expand-icon">{{ forumEntry.expanded ? '−' : '+' }}</span>
      </button>
      <h4 class="header-title">{{ forumEntry.title }}</h4>
      <div class="header-actions">
        <slot name="action-button"></slot>
      </div>
    </div>
    <div v-if="forumEntry.expanded" class="forum-card-details">
      <div class="details-layout">
        <div class="forum-info-container">
          <!-- Creator info -->
          <div class="info-row">
            <span class="info-label">ERSTELLER</span>
            <span class="info-value">{{ forumEntry.creatorUserName }}</span>
          </div>

          <!-- Created at -->
          <div class="info-row">
            <span class="info-label">ERSTELLT AM</span>
            <span class="info-value">{{ formattedDate }}</span>
          </div>

          <!-- Content -->
          <div v-if="forumEntry.content" class="content-section">
            <span class="content-label">INHALT</span>
            <p class="content-text">{{ forumEntry.content }}</p>
          </div>

          <!-- Mobile only: Comments button -->
          <button class="mobile-comments-button" @click="openCommentModal">
            <span class="comments-count">{{ forumEntry.comments.length }}</span>
            Kommentare anzeigen
          </button>
        </div>

        <!-- Desktop: Comments sidebar -->
        <div class="comments-sidebar">
          <CommentSection
            :comments="forumEntry.comments"
            :item-id="forumEntry.id"
            api-endpoint="api/forum/CommentOnForumEntry/"
            @comment-sent="handleCommentSent"
          />
        </div>
      </div>
    </div>

    <!-- Mobile modal for comments -->
    <CommentModal 
      :is-open="isCommentModalOpen"
      :comments="forumEntry.comments"
      :item-id="forumEntry.id"
      api-endpoint="api/forum/CommentOnForumEntry/"
      @close="closeCommentModal"
      @comment-sent="handleCommentSent"
    />
  </li>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { ForumEntry } from '@/types/ForumInterfaces';
import CommentSection from '@/components/CommentSection.vue';
import CommentModal from '@/components/CommentModal.vue';

export default defineComponent({
  components: {
    CommentSection,
    CommentModal
  },
  props: {
    forumEntry: {
      type: Object as PropType<ForumEntry>,
      required: true
    },
    isOwner: {
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
    formattedDate(): string {
      const date = new Date(this.forumEntry.createdAt);
      return date.toLocaleDateString('de-DE', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      });
    }
  },
  methods: {
    handleCommentSent(payload: any) {
      this.$emit('comment-sent', {
        forumEntryId: payload.itemId,
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

/* Header section */
.forum_card_header {
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

/* Header actions container (edit/delete buttons) */
.header-actions {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-xs);
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

/* Details section */
.forum-card-details {
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

.forum-info-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
}

/* Info row (key-value pairs) */
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

/* Content section */
.content-section {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  margin-top: var(--space-sm);
}

.content-label {
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
}

.content-text {
  font-size: var(--text-sm);
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding-right: 0;
  border-radius: var(--radius-interactive);
  margin: 0;
}

/* Mobile comments button */
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

/* Comments sidebar (desktop only) */
.comments-sidebar {
  display: none;
}

/* Desktop: Side-by-side layout */
@media (min-width: 785px) {
  .details-layout {
    flex-direction: row;
    gap: var(--space-md);
    align-items: flex-start;
  }

  .forum-info-container {
    flex: 1 1 50%;
    min-width: 0;
  }

  .comments-sidebar {
    display: block;
    flex: 1 1 50%;
    min-width: 0;
  }

  .mobile-comments-button {
    display: none;
  }
}
</style>
