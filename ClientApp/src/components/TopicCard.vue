<template>
  <li class="card_scroll_container">
    <p v-if="isHighlighted" class="most_liked_hint">Publikumsliebling</p>
    <div :class="{ topic_card_header: true, most_liked_highlight: isHighlighted }">
      <button class="action_button" @click="$emit('toggle-details')">
        <img :src="topic.expanded ? 'collapse.svg' : '/expand.svg'" alt="Expand">
      </button>
      <h4 :class="{ topic_card_header_toggled: topic.expanded }">{{ topic.title }}</h4>
      <slot name="action-button"></slot>
    </div>
    <div v-if="topic.expanded" class="topic-card-details">
      <div style="display: flex; flex-direction: row;">
        <p class="topic-card-presentation-time-in-minutes">{{ topic.presentationTimeInMinutes }}'</p>
        <p class="description">{{ topic.description }}</p>
        <slot name="actions"></slot>
      </div>
      <slot name="presenter"></slot>
      <CommentSection
        :comments="topic.comments"
        :item-id="topic.id"
        api-endpoint="api/topic/CommentOnTopic/"
        @comment-sent="handleCommentSent"
      />
    </div>
  </li>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {MyTopic, ForeignTopic} from '@/types/TopicInterfaces';
import CommentSection from '@/components/CommentSection.vue';

export default defineComponent({
  components: {
    CommentSection
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
  methods: {
    handleCommentSent(payload: any) {
      this.$emit('comment-sent', {
        topicId: payload.itemId,
        comment: payload.comment,
        content: payload.content
      });
    }
  }
});
</script>

<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css"></style>
