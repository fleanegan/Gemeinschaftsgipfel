<template>
  <teleport to="body">
    <transition name="modal-fade">
      <div v-if="isOpen" class="modal-overlay" @click.self="closeModal">
        <div class="modal-container">
          <div class="modal-header">
            <h3>Kommentare</h3>
            <button class="close-button" @click="closeModal">×</button>
          </div>
          <div class="modal-content">
            <CommentSection 
              :comments="comments"
              :item-id="itemId"
              :api-endpoint="apiEndpoint"
              @comment-sent="$emit('comment-sent', $event)"
            />
          </div>
        </div>
      </div>
    </transition>
  </teleport>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';
import type {Comment} from '@/types/TopicInterfaces';
import CommentSection from './CommentSection.vue';

export default defineComponent({
  components: { CommentSection },
  props: {
    isOpen: { type: Boolean, required: true },
    comments: { type: Array as PropType<Comment[]>, required: true },
    itemId: { type: String, required: true },
    apiEndpoint: { type: String, required: true }
  },
  emits: ['close', 'comment-sent'],
  methods: {
    closeModal() {
      this.$emit('close');
    },
    handleKeydown(event: KeyboardEvent) {
      if (!this.isOpen) return;
      
      if (event.key === 'Escape') {
        event.preventDefault();
        this.closeModal();
      }
    }
  },
  watch: {
    isOpen(newVal) {
      document.body.style.overflow = newVal ? 'hidden' : '';
    }
  },
  mounted() {
    document.addEventListener('keydown', this.handleKeydown);
  },
  beforeUnmount() {
    document.body.style.overflow = '';
    document.removeEventListener('keydown', this.handleKeydown);
  }
});
</script>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  z-index: 9999;
  backdrop-filter: blur(2px);
}

.modal-container {
  background-color: var(--color-background);
  border-radius: var(--radius-sharp) var(--radius-sharp) 0 0;
  width: 100%;
  max-height: 85vh;
  display: flex;
  flex-direction: column;
  animation: slide-up 0.3s ease-out;
}

@keyframes slide-up {
  from { transform: translateY(100%); }
  to { transform: translateY(0); }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-md) var(--space-md);
  border-bottom: 1px solid var(--color-border-light);
  background-color: var(--color-background-soft);
}

.modal-header h3 {
  margin: 0;
  font-size: var(--text-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-primary);
}

.close-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  padding: 0;
  border: none;
  background: none;
  font-size: 2rem;
  line-height: 1;
  color: var(--color-main-text);
  cursor: pointer;
  border-radius: var(--radius-interactive);
  transition: background-color 0.2s ease;
}

.close-button:hover {
  background-color: var(--color-border-light);
}

.modal-content {
  flex: 1;
  overflow-y: auto;
  padding: var(--space-md);
}

/* Desktop: Centered modal instead of bottom sheet */
@media (min-width: 785px) {
  .modal-overlay {
    align-items: center;
  }
  
  .modal-container {
    border-radius: var(--radius-sharp);
    max-width: 600px;
    max-height: 80vh;
    animation: fade-in 0.3s ease-out;
  }

  @keyframes fade-in {
    from {
      opacity: 0;
      transform: scale(0.95);
    }
    to {
      opacity: 1;
      transform: scale(1);
    }
  }
}
</style>
