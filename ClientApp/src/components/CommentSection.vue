<template>
  <div class="comments-container">
    <div class="comments-header">
      <span class="comments-label">KOMMENTARE</span>
    </div>
    <ul class="comments-list">
      <li v-for="comment in comments" :key="comment.createdAt" class="comment-item">
        <div class="comment-header">
          <span class="comment-author">{{ comment.creatorUserName }}</span>
          <span class="comment-timestamp">{{ formatDateTime(comment.createdAt) }}</span>
        </div>
        <p class="comment-content">{{ comment.content }}</p>
      </li>
    </ul>
    <div class="comment-input-container">
      <input v-model="content"
             class="comment-input"
             placeholder="Kommentar schreiben..."
             @keyup.enter="handleSendComment"/>
      <button class="send-button"
              @click="handleSendComment"
              :disabled="!content.trim()">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="22" y1="2" x2="11" y2="13"></line>
          <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
        </svg>
      </button>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {Comment} from '@/types/TopicInterfaces';
import {apiClient} from '@/services/api';
import {formatDateTime} from '@/utils/dateFormatter';

export default defineComponent({
  data() {
    return {
      content: '',
    };
  },
  props: {
    comments: {
      type: Array as PropType<Comment[]>,
      required: true
    },
    itemId: {
      type: String,
      required: true
    },
    apiEndpoint: {
      type: String,
      required: true
    }
  },
  emits: ['comment-sent'],
  methods: {
    async handleSendComment() {
      try {
        if (!this.content) {
          return;
        }
        const response = await apiClient.post(this.apiEndpoint, {
          [this.getIdFieldName()]: this.itemId,
          Content: this.content
        });
        this.$emit('comment-sent', {
          itemId: this.itemId,
          comment: response.data,
          content: this.content
        });
        this.content = '';
      } catch (error) {
        console.error('Error sending comment:', error);
      }
    },
    getIdFieldName(): string {
      // Determine field name based on endpoint
      if (this.apiEndpoint.includes('topic')) {
        return 'TopicId';
      } else if (this.apiEndpoint.includes('rideshare')) {
        return 'RideShareId';
      }
      return 'Id';
    },
    formatDateTime
  }
});
</script>

<style scoped>
.comments-container {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border-light);
}

.comments-header {
  margin-bottom: 1rem;
}

.comments-label {
  font-size: 0.6875rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
}

.comments-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
  list-style: none;
  padding: 0;
}

.comment-item {
  background-color: var(--color-nuance-light);
  padding: 0.75rem;
  border-radius: 4px;
}

.comment-header {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  margin-bottom: 0.375rem;
}

.comment-author {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--color-primary);
}

.comment-timestamp {
  font-size: 0.75rem;
  color: var(--color-main-text);
  opacity: 0.6;
}

.comment-content {
  font-size: 0.9375rem;
  color: var(--color-primary);
  line-height: 1.5;
  word-wrap: break-word;
  overflow-wrap: break-word;
  margin: 0;
}

.comment-input-container {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.comment-input {
  flex: 1;
  padding: 0.75rem;
  border: 1px solid var(--color-border-light);
  border-radius: 4px;
  font-size: 0.9375rem;
  color: var(--color-primary);
  background-color: var(--color-background);
  transition: border-color 0.2s ease;
}

.comment-input:focus {
  outline: none;
  border-color: var(--color-primary);
}

.comment-input::placeholder {
  color: var(--color-main-text);
  opacity: 0.5;
}

.send-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  border: none;
  border-radius: 4px;
  background-color: var(--color-primary);
  color: var(--color-background);
  cursor: pointer;
  transition: background-color 0.2s ease, opacity 0.2s ease;
  flex-shrink: 0;
}

.send-button:hover:not(:disabled) {
  background-color: #005a39;
}

.send-button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.send-button svg {
  width: 18px;
  height: 18px;
}
</style>
