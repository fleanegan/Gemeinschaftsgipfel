<template>
  <div class="input-forum-container">
    <h1>{{ isEditing ? 'Beitrag bearbeiten' : 'Neuer Beitrag' }}</h1>
    <form @submit.prevent="submit">
      <div class="form-group">
        <label for="title">Titel</label>
        <input 
          id="title" 
          v-model="title" 
          class="form-input" 
          type="text"
          maxlength="150"
          required
        />
        <p v-if="isTitleEmpty" class="error-message">Der Titel darf nicht leer sein</p>
        <p v-if="isTitleTooLong" class="error-message">Der Titel darf maximal 150 Zeichen lang sein</p>
      </div>
      
      <div class="form-group">
        <label for="content">Inhalt</label>
        <textarea 
          id="content" 
          v-model="content" 
          class="form-input" 
          rows="10"
          maxlength="10000"
          required
        ></textarea>
        <p v-if="isContentEmpty" class="error-message">Der Inhalt darf nicht leer sein</p>
        <p v-if="isContentTooLong" class="error-message">Der Inhalt darf maximal 10000 Zeichen lang sein</p>
      </div>
      
      <div class="button-container">
        <button type="button" class="abort-button" @click="abort">Abbrechen</button>
        <button type="submit" class="submit-button" :disabled="!canSubmit">Absenden</button>
      </div>
    </form>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { forumService } from '@/services/api';

export default defineComponent({
  props: {
    forumId: {
      type: String,
      required: false
    }
  },
  data() {
    return {
      title: '',
      content: ''
    };
  },
  computed: {
    isEditing(): boolean {
      return !!this.forumId;
    },
    isTitleEmpty(): boolean {
      return this.title.trim().length === 0;
    },
    isContentEmpty(): boolean {
      return this.content.trim().length === 0;
    },
    isTitleTooLong(): boolean {
      return this.title.length > 150;
    },
    isContentTooLong(): boolean {
      return this.content.length > 10000;
    },
    canSubmit(): boolean {
      return !this.isTitleEmpty && 
             !this.isContentEmpty && 
             !this.isTitleTooLong && 
             !this.isContentTooLong;
    }
  },
  methods: {
    async submit() {
      if (!this.canSubmit) return;
      
      try {
        if (this.isEditing) {
          await forumService.updateForumEntry({
            id: this.forumId!,
            title: this.title,
            content: this.content
          });
        } else {
          await forumService.createForumEntry({
            title: this.title,
            content: this.content
          });
        }
        
        this.$router.push('/forum');
      } catch (error) {
        console.error('Error submitting forum entry:', error);
      }
    },
    abort() {
      this.$router.push('/forum');
    }
  },
  async beforeCreate() {
    if (this.$props.forumId) {
      try {
        const existingEntry = await forumService.getForumEntry(this.$props.forumId);
        this.title = existingEntry.title;
        this.content = existingEntry.content;
      } catch (error) {
        console.error('Error loading existing forum entry:', error);
      }
    }
  }
});
</script>

<style scoped>
.input-forum-container {
  max-width: 800px;
  margin: 0 auto;
  padding: var(--spacing-lg);
}

h1 {
  margin-bottom: var(--spacing-lg);
  color: var(--color-primary);
}

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: var(--spacing-lg);
}

label {
  font-weight: var(--font-weight-semibold);
  margin-bottom: var(--spacing-xs);
  color: var(--color-main-text);
}

.form-input {
  padding: var(--spacing-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-interactive);
  font-size: var(--text-base);
  font-family: inherit;
  background-color: var(--color-background);
  color: var(--color-main-text);
  transition: border-color 0.2s ease;
}

.form-input:focus {
  outline: none;
  border-color: var(--color-primary);
}

textarea.form-input {
  resize: vertical;
  min-height: 200px;
}

.error-message {
  color: #dc2626;
  font-size: var(--text-sm);
  margin-top: var(--spacing-xs);
  margin-bottom: 0;
}

.button-container {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
  margin-top: var(--spacing-xl);
}

.abort-button,
.submit-button {
  padding: var(--spacing-sm) var(--spacing-xl);
  border: none;
  border-radius: var(--radius-interactive);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: all 0.2s ease;
}

.abort-button {
  background-color: var(--color-background-soft);
  color: var(--color-main-text);
}

.abort-button:hover {
  background-color: var(--color-border);
}

.submit-button {
  background-color: var(--color-primary);
  color: var(--color-text-bright);
}

.submit-button:hover:not(:disabled) {
  opacity: 0.9;
}

.submit-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
