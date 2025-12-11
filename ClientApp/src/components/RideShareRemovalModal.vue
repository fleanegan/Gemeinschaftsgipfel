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
            <div class="modal-actions-vertical">
              <button class="option-button cancel-option" @click="$emit('cancel-ride')">
                Fahrt absagen
              </button>
              <button class="option-button delete-option" @click="$emit('delete-ride')">
                Fahrt löschen
              </button>
              <button class="cancel-button" @click="$emit('cancel')">
                Abbrechen
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
  padding: var(--space-md);
}

.modal-dialog {
  background-color: var(--color-background);
  border-radius: var(--radius-interactive);
  max-width: 500px;
  width: 100%;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.modal-header {
  padding: var(--space-lg);
  border-bottom: 1px solid var(--color-border-light);
}

.modal-header h3 {
  margin: 0;
  font-size: var(--text-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-primary);
}

.modal-content {
  padding: var(--space-lg);
}

.modal-content p {
  margin: 0;
  font-size: var(--text-base);
  color: var(--color-main-text);
  line-height: 1.6;
}

.modal-actions-vertical {
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  border-top: 1px solid var(--color-border-light);
}

.option-button {
  padding: var(--space-md);
  border: none;
  border-radius: var(--radius-interactive);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: center;
}

.cancel-option {
  background-color: #fff3e0;
  color: #e65100;
}

.cancel-option:hover {
  background-color: #ffe0b2;
}

.delete-option {
  background-color: #ffebee;
  color: #c62828;
}

.delete-option:hover {
  background-color: #ffcdd2;
}

.cancel-button {
  padding: var(--space-md);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-interactive);
  background-color: transparent;
  color: var(--color-main-text);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: all 0.2s ease;
}

.cancel-button:hover {
  background-color: var(--color-nuance-light);
}

/* Mobile responsiveness */
@media (max-width: 600px) {
  .modal-dialog {
    margin: 0;
    border-radius: var(--radius-interactive) var(--radius-interactive) 0 0;
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    max-width: 100%;
  }
}
</style>
