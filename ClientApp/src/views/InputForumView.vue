<template>
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
      <button type="button" class="abort-button" @click="abort">Verwerfen</button>
      <button type="submit" class="submit-button" :disabled="!canSubmit">Abschicken</button>
    </div>
  </form>
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
.form-group {
  display: flex;
  flex-direction: column;
  padding: 0 1rem 1rem 1rem;
}

.error-message {
  color: red;
}

.button-container {
  display: flex;
  flex-direction: row;
}

.submit-button {
  margin-left: auto;
  margin-right: 1rem;
}
</style>
