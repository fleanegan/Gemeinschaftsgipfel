<template>
  <div class="forum wide-content">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Forum</h1>
    
    <section class="forum-section">
      <h2>Alle Beiträge</h2>
      <ul class="list">
        <ForumCard
          v-for="(entry, index) in forumEntries"
          :key="entry.id"
          :forum-entry="entry"
          :is-owner="isOwner(entry)"
          @toggle-details="toggleDetails(index)"
          @comment-sent="handleCommentSent"
        >
          <template v-if="isOwner(entry)" #action-button>
            <div class="forum_card_details_owner_actions">
              <button class="action_button" @click="confirmDelete(entry.id)">
                <img alt="Delete" src="/empty_delete.svg">
              </button>
              <button class="action_button" @click="editForumEntry(entry.id)">
                <img alt="Edit" src="/empty_edit_no_border.svg">
              </button>
            </div>
          </template>
        </ForumCard>
        <li class="list-end-message">Ende der Liste</li>
      </ul>
      <hr>
      <div class="add-button-container">
        <button class="submit-button" @click="addNewForumEntry">Neuer Beitrag</button>
      </div>
    </section>

    <ConfirmationModal
      :is-open="confirmationModalOpen"
      title="Beitrag löschen"
      message="Möchten Sie diesen Forumsbeitrag wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden."
      confirm-text="Löschen"
      cancel-text="Abbrechen"
      confirm-button-class="danger-button"
      @confirm="removeForumEntry"
      @cancel="cancelDelete"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import type { ForumEntry } from '@/types/ForumInterfaces';
import type { Comment } from '@/types/TopicInterfaces';
import ForumCard from '@/components/ForumCard.vue';
import ConfirmationModal from '@/components/ConfirmationModal.vue';
import { scrollToTopMixin } from '@/mixins/scrollToTop';
import { forumService } from '@/services/api';
import { useAuthStore } from '@/store/auth';

export default defineComponent({
  components: { ForumCard, ConfirmationModal },
  mixins: [scrollToTopMixin],
  data() {
    return {
      forumEntries: [] as ForumEntry[],
      confirmationModalOpen: false,
      forumEntryToDelete: null as string | null
    };
  },
  computed: {
    currentUserName(): string {
      const authStore = useAuthStore();
      return authStore.userName || '';
    }
  },
  methods: {
    async fetchData() {
      this.forumEntries = await forumService.getAllForumEntries();
    },
    async toggleDetails(index: number): Promise<void> {
      const entry = this.forumEntries[index];
      if (!entry) return;
      if (entry.isLoading) return;
      
      try {
        entry.expanded = !entry.expanded;
        if (entry.expanded) {
          entry.isLoading = true;
          entry.comments = await forumService.getComments(entry.id);
          
          entry.comments.sort((a: Comment, b: Comment) => {
            return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
          });
        } else {
          entry.comments = [];
        }
      } catch (error) {
        console.error('Error toggling details:', error);
        entry.expanded = !entry.expanded;
      } finally {
        entry.isLoading = false;
      }
    },
    editForumEntry(id: string) {
      this.$router.push(`/forum/edit/${id}`);
    },
    confirmDelete(id: string) {
      this.forumEntryToDelete = id;
      this.confirmationModalOpen = true;
    },
    async removeForumEntry() {
      if (!this.forumEntryToDelete) return;
      
      try {
        await forumService.deleteForumEntry(this.forumEntryToDelete);
        this.confirmationModalOpen = false;
        this.forumEntryToDelete = null;
        await this.fetchData();
      } catch (error) {
        console.error('Error deleting forum entry:', error);
      }
    },
    cancelDelete() {
      this.confirmationModalOpen = false;
      this.forumEntryToDelete = null;
    },
    addNewForumEntry() {
      this.$router.push('/forum/add');
    },
    async handleCommentSent(payload: any) {
      const entry = this.forumEntries.find(e => e.id === payload.forumEntryId);
      if (entry) {
        if (payload.comment) {
          entry.comments.unshift(payload.comment);
        } else {
          this.refreshForumEntryComments(entry);
        }
      }
    },
    async refreshForumEntryComments(entry: ForumEntry): Promise<void> {
      if (entry.expanded) {
        entry.comments = await forumService.getComments(entry.id);
        entry.comments.sort((a: Comment, b: Comment) => {
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
      }
    },
    isOwner(entry: ForumEntry): boolean {
      return entry.creatorUserName.toLowerCase() === this.currentUserName.toLowerCase();
    }
  },
  mounted() {
    this.fetchData();
  }
});
</script>

<style scoped>
.floating_scroll_to_top_hidden {
  margin-left: auto;
  height: 3.3rem;
  width: 75%;
  position: sticky;
  top: 0;
  left: 0;
  z-index: 9999;
  display: none;
}

.floating_scroll_to_top_shown {
  display: flex;
  flex-direction: row;
  align-content: center;
  justify-content: right;
  background-color: transparent;
}

@media (max-width: 900px) {
  .floating_scroll_to_top_shown {
    background-color: var(--color-background);
  }
}

.forum {
  background-color: var(--color-background);
}

.forum h1 {
  margin-left: 0;
}

.forum h2 {
  margin-left: 0;
}

.forum-section {
  margin-bottom: var(--spacing-xl);
}

.list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.list-end-message {
  text-align: center;
  color: var(--color-text-muted);
  padding: var(--spacing-lg);
  font-style: italic;
}

.add-button-container {
  display: flex;
  justify-content: center;
  margin-top: var(--spacing-lg);
}

.submit-button {
  padding: var(--spacing-sm) var(--spacing-xl);
  background-color: var(--color-primary);
  color: var(--color-text-bright);
  border: none;
  border-radius: var(--radius-interactive);
  font-weight: var(--font-weight-semibold);
  font-size: var(--text-base);
  cursor: pointer;
  transition: opacity 0.2s ease;
}

.submit-button:hover {
  opacity: 0.9;
}

.forum_card_details_owner_actions {
  display: flex;
  gap: var(--space-xs);
}

.action_button {
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

.action_button:hover {
  background-color: var(--color-border-light);
}

.action_button img {
  width: 1.25rem;
  height: 1.25rem;
}

hr {
  border: none;
  border-top: 1px solid var(--color-border-light);
  margin: var(--spacing-lg) 0;
}
</style>
