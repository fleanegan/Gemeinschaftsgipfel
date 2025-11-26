<template>
  <div class="comments-container">
    <p class="comments-title">Kommentare:</p>
    <ul class="comments-list">
      <div v-for="comment in comments" :key="comment.createdAt" class="comment-item">
        <div class="flex-row">
          <p class="comment-author">{{ comment.creatorUserName }}</p>
          <p class="comment-timestamp">({{ formatDateTime(comment.createdAt) }})</p>
        </div>
        <p class="comment-content">{{ comment.content }}</p>
      </div>
    </ul>
    <div class="flex-row" style="margin-bottom: 2rem">
      <input v-model="content"
             class="comment-input"
             placeholder="Kommentar schreiben ..."/>
      <button class="action_button comment-send-button"
              @click="handleSendComment">Senden
      </button>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {Comment} from '@/types/TopicInterfaces';
import axios from "axios";
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
        const response = await axios.post(this.apiEndpoint, {
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
  margin-top: 0.5rem;
}

.comments-title {
  margin-left: 3rem;
  margin-top: 0.1rem;
}

.comments-list {
  margin-bottom: 1rem;
  margin-left: 0.5rem;
  max-width: 100%;
}

.comment-item {
  max-width: 90%;
  margin-left: 0.5rem;
  font-size: small;
  margin-top: 0.25rem;
  margin-bottom: 0.5rem;
}

.flex-row {
  display: flex;
  flex-direction: row;
}

.comment-author {
  font-weight: bold;
}

.comment-timestamp {
  margin-left: 0.25rem;
}

.comment-content {
  margin-left: 1rem;
  margin-top: 0.15rem;
  word-wrap: break-word;
  overflow-wrap: break-word;
  word-break: break-all;
  max-width: 100%;
}

.comment-input {
  border-color: var(--main-color-primary);
  color: var(--main-color-primary);
  margin-left: 3rem;
  border-style: solid;
  border-width: 0.01rem;
  border-radius: 0.2rem;
}

.comment-send-button {
  color: var(--color-primary);
}
</style>
