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
                Abbrechen
              </button>
              <button class="option-button delete-option" @click="$emit('delete-ride')">
                Löschen
              </button>
              <button class="option-button cancel-option" @click="$emit('cancel-ride')">
                Absagen
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
      default: 'Fahrt verwalten'
    },
    message: {
      type: String,
      default: 'Was möchtest du tun?'
    }
  },
  emits: ['cancel-ride', 'delete-ride', 'cancel'],
  watch: {
    isOpen(newVal) {
      document.body.style.overflow = newVal ? 'hidden' : '';
      if (newVal) {
        this.$nextTick(() => {
          const cancelButton = this.$el?.querySelector('.cancel-button');
          if (cancelButton) {
            (cancelButton as HTMLElement).focus();
          }
        });
      }
    }
  },
  mounted() {
    document.addEventListener('keydown', this.handleKeydown);
  },
  beforeUnmount() {
    document.body.style.overflow = '';
    document.removeEventListener('keydown', this.handleKeydown);
  },
  methods: {
    handleKeydown(event: KeyboardEvent) {
      if (!this.isOpen) return;
      
      if (event.key === 'Escape') {
        event.preventDefault();
        this.$emit('cancel');
      }
    }
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
  color: var(--color-primary);
  line-height: 1.6;
  font-size: var(--text-base);
}

.modal-actions {
  padding: var(--space-md);
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  background-color: var(--color-background-soft);
  border-top: 1px solid var(--color-border-light);
}

.option-button {
  padding: var(--space-sm) var(--space-lg);
  border: none;
  border-radius: var(--radius-interactive);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: center;
}

.cancel-option {
  background-color: #dc2626;
  color: white;
}

.cancel-option:hover {
  background-color: #b91c1c;
}

.delete-option {
  background-color: white;
  color: var(--color-primary);
  border: 1px solid var(--color-border-light);
}

.delete-option:hover {
  background-color: var(--color-nuance-light);
}

.cancel-button {
  padding: var(--space-sm) var(--space-lg);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-interactive);
  background-color: var(--color-background);
  color: var(--color-primary);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: all 0.2s ease;
}

.cancel-button:hover {
  background-color: var(--color-border-light);
}

/* Mobile responsiveness */
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
