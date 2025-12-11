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
                <img alt="Delete" src="/trash_bin.svg">
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
      message="Möchtest du diesen Forumsbeitrag wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden."
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
  flex: 1;
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
}

.list {
  background-color: var(--color-background-soft);
  padding: var(--space-md);
  border-radius: var(--radius-sharp);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  list-style: none;
  margin: 0;
}

/* Small screens: Reduce list padding */
@media (max-width: 400px) {
  .list {
    padding: var(--space-sm) var(--space-sm) !important;
    gap: var(--space-xs) !important;
  }
}

.list-end-message {
  text-align: center;
  padding: var(--space-md);
  color: var(--color-main-text);
  opacity: 0.7;
  font-size: var(--text-sm);
}

.add-button-container {
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
}

.submit-button {
  margin-top: 0;
  margin-left: auto;
  border-top-left-radius: 0;
  border-top-right-radius: 0;
  border-bottom-left-radius: var(--radius-interactive);
  border-bottom-right-radius: var(--radius-interactive);
  margin-right: 0;
  white-space: nowrap;
  padding-left: var(--space-lg);
  padding-right: var(--space-lg);
  width: auto;
}

.forum_card_details_owner_actions {
  display: flex;
  flex-direction: row;
  align-content: center;
  align-items: center;
  gap: var(--space-sm);
}

.forum_card_details_owner_actions button {
  margin-left: var(--space-sm);
}

.action_button {
  cursor: pointer;
  background: none;
  border-style: none;
  border-radius: var(--radius-interactive);
  font-weight: bold;
  display: flex;
  place-items: center;
}


</style>
