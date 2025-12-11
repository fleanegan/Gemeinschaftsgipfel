<template>
  <teleport to="body">
    <transition name="modal-fade">
      <div v-if="isOpen" class="modal-overlay" @click.self="$emit('cancel')">
        <transition name="slide-up">
          <div v-if="isOpen" class="modal-dialog">
            <div class="modal-header">
              <h3>{{ title }}</h3>
            </div>
            <div class="modal-content">
              <p>{{ message }}</p>
            </div>
            <div class="modal-actions">
              <button class="cancel-button" @click="$emit('cancel')">
                {{ cancelText }}
              </button>
              <button :class="['confirm-button', confirmButtonClass]" @click="$emit('confirm')">
                {{ confirmText }}
              </button>
            </div>
          </div>
        </transition>
      </div>
    </transition>
  </teleport>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  props: {
    isOpen: {
      type: Boolean,
      required: true
    },
    title: {
      type: String,
      required: true
    },
    message: {
      type: String,
      required: true
    },
    confirmText: {
      type: String,
      default: 'Bestätigen'
    },
    cancelText: {
      type: String,
      default: 'Abbrechen'
    },
    confirmButtonClass: {
      type: String,
      default: 'danger-button'
    }
  },
  emits: ['confirm', 'cancel'],
  watch: {
    isOpen(newVal) {
      document.body.style.overflow = newVal ? 'hidden' : '';
    }
  },
  beforeUnmount() {
    document.body.style.overflow = '';
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

.slide-up-enter-active,
.slide-up-leave-active {
  transition: transform 0.3s ease-out;
}

.slide-up-enter-from {
  transform: translateY(100%);
}

.slide-up-leave-to {
  transform: translateY(100%);
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  backdrop-filter: blur(2px);
}

.modal-dialog {
  background-color: var(--color-background);
  border-radius: var(--radius-sharp);
  width: 90%;
  max-width: 28rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
  overflow: hidden;
}

.modal-header {
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

.modal-content {
  padding: var(--space-md);
}

.modal-content p {
  margin: 0;
  color: var(--color-main-text);
  line-height: 1.6;
  font-size: var(--text-base);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  padding: var(--space-md);
  background-color: var(--color-background-soft);
  border-top: 1px solid var(--color-border-light);
}

.cancel-button,
.confirm-button {
  padding: var(--space-sm) var(--space-lg);
  border: none;
  border-radius: var(--radius-interactive);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: all 0.2s ease;
}

.cancel-button {
  background-color: var(--color-background);
  color: var(--color-main-text);
  border: 1px solid var(--color-border-light);
}

.cancel-button:hover {
  background-color: var(--color-border-light);
}

.confirm-button {
  background-color: var(--color-primary);
  color: var(--color-text-bright);
}

.confirm-button:hover {
  opacity: 0.9;
}

.danger-button {
  background-color: #dc2626;
  color: white;
}

.danger-button:hover {
  background-color: #b91c1c;
}

@media (max-width: 785px) {
  .modal-overlay {
    align-items: flex-end;
  }

  .modal-dialog {
    width: 100%;
    max-width: 100%;
    border-radius: var(--radius-sharp) var(--radius-sharp) 0 0;
  }
}
</style>
