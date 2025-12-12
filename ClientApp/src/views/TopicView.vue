<template>
  <div class="topic wide-content">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Zeig uns was 'ne Harke ist!</h1>
    <div class="instructions-formats-row">
      <div class="instructions-wrapper">
        <InstructionCards :instructions="instructions" />
      </div>
      <TopicTypeExplanationStack />
    </div>
    
    <div class="sections-container">
      <!-- Left section: Meine Vorschläge -->
      <section class="topic-section">
        <h2>Meine Vorschläge</h2>
        <ul class="list">
          <TopicCard
              v-for="(item, index) in myTopics"
              :key="item.id"
              :topic="item"
              :isHighlighted="item === mostLikedTopic && mostLikedTopic?.votes > 0"
              @toggle-details="toggleDetails(myTopics, index)"
              @comment-sent="handleCommentSent"
          >
            <template #action-button>
              <div class="topic_card_details_owner_actions">
                <button class="action_button" @click="confirmDeleteTopic(item.id)">
                  <img alt="Delete" src="/trash_bin.svg">
                </button>
                <button class="action_button" @click="editTopic(item.id)">
                  <img alt="Edit" src="/empty_edit_no_border.svg">
                </button>
              </div>
            </template>
          </TopicCard>
        </ul>
        <hr>
        <div id="owner_action" class="my-topics-add-button-container">
          <button class="submit-button" @click="addNewTopic">Neuer Vorschlag</button>
        </div>
      </section>
      
      <!-- Right section: Vorschläge der Anderen -->
      <section class="topic-section">
        <h2>Vorschläge der Anderen</h2>
        <ul class="list">
          <TopicCard
              v-for="(item, index) in foreignTopics"
              :key="item.id"
              :topic="item"
              :isHighlighted="false"
              @toggle-details="toggleDetails(foreignTopics, index)"
              @comment-sent="handleCommentSent"
          >
            <template #vote-button>
              <button class="action_button" @click="toggleVote(index)">
                <img :src="item.didIVoteForThis ? '/full_heart.svg' : '/empty_heart.svg'" alt="Vote">
              </button>
            </template>
            <template #presenter>
              <div class="info-row">
                <span class="info-label">ES TRÄGT VOR</span>
                <span class="info-value">{{ item.presenterUserName }}</span>
              </div>
            </template>
          </TopicCard>
          <li class="list-end-message">Ende der Liste. Danke fürs Abstimmen!</li>
        </ul>
      </section>
    </div>

    <ConfirmationModal
      :is-open="confirmationModalOpen"
      title="Vorschlag löschen"
      message="Möchtest du diesen Vorschlag wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden."
      confirm-text="Löschen"
      cancel-text="Abbrechen"
      confirm-button-class="danger-button"
      @confirm="removeTopic"
      @cancel="cancelDelete"
    />
  </div>
</template>


<script lang="ts">
import {defineComponent} from 'vue';
import type {ForeignTopic, MyTopic} from "@/types/TopicInterfaces";
import type {Comment} from "@/types/TopicInterfaces";
import TopicCard from "@/components/TopicCard.vue";
import ConfirmationModal from '@/components/ConfirmationModal.vue';
import InstructionCards from '@/components/InstructionCards.vue';
import TopicTypeExplanationStack from '@/components/TopicTypeExplanationStack.vue';
import {scrollToTopMixin} from '@/mixins/scrollToTop';
import {topicService} from '@/services/api';
import {formatDateTime} from '@/utils/dateFormatter';


export default defineComponent({
  components: {TopicCard, ConfirmationModal, InstructionCards, TopicTypeExplanationStack},
  mixins: [scrollToTopMixin],
  data() {
    return {
      foreignTopics: [] as ForeignTopic[],
      myTopics: [] as MyTopic[],
      confirmationModalOpen: false,
      topicToDelete: null as string | null,
      instructions: [
        {
          title: 'Inhalt ausdenken',
          content: "Reite dein Steckenpferd und zeig' uns, was dich begeistert! Ob Trick 17, dein Promotionsthema oder Haekeltipps, wir sind gespannt."
        },
        {
          title: 'Entscheidung treffen',
          content: 'Geht es dir wie uns, du kannst dich kaum entscheiden, welches deiner vielen Herzensthemen du praesentieren sollst? Trag alle Themen ein, lass die Gemeinschaft waehlen und hilf selbst mit deiner Stimme!'
        },
        {
          title: 'Gemeinsam staunen',
          content: 'Das Ziel ist es, zusammen unsere Vielfalt zu geniessen. Lass Leistungsdruck und Lampenfieber zuhause, denn es erwartet dich ein wohlwollendes Publikum :)'
        }
      ]
    };
  },
  methods: {
    async fetchData() {
      this.myTopics = await topicService.getMyTopics();
      this.foreignTopics = await topicService.getForeignTopics();
    },
async toggleDetails(topic: MyTopic[] | ForeignTopic[], index: number): Promise<void> {
  const item = topic[index];
  if (!item) return;
  if (item.isLoading) return;
  try {
    item.expanded = !item.expanded;
    if (item.expanded) {
      item.isLoading = true;
      item.comments = await topicService.getComments(item.id);

      item.comments.sort((a: Comment, b: Comment) => {
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      });
    } else {
      item.comments = [];
    }
  } catch (error) {
    console.error('Error toggling details:', error);
    // Revert expanded state on error
    item.expanded = !item.expanded;
  } finally {
    // Clear loading state
    item.isLoading = false;
  }
},
    async toggleVote(index: number): Promise<void> {
      const topic = this.foreignTopics[index];
      if (!topic) return;
      if (!topic.didIVoteForThis)
        await topicService.addVote(topic.id);
      else
        await topicService.removeVote(topic.id);
      topic.didIVoteForThis = !topic.didIVoteForThis;
    },
    editTopic(topicId: string): void {
      this.$router.push({
        name: 'Thema bearbeiten',
        params: {
          'topicId': topicId,
        }
      });
    },
    formatDateTime,
    confirmDeleteTopic(topicId: string) {
      this.topicToDelete = topicId;
      this.confirmationModalOpen = true;
    },
    async removeTopic() {
      if (!this.topicToDelete) return;
      
      try {
        await topicService.deleteTopic(this.topicToDelete);
        this.confirmationModalOpen = false;
        this.topicToDelete = null;
        await this.fetchData();
      } catch (error) {
        console.error('Error deleting topic:', error);
      }
    },
    cancelDelete() {
      this.confirmationModalOpen = false;
      this.topicToDelete = null;
    },
    addNewTopic() {
      this.$router.push("/topic/add");
    },
    async handleCommentSent(payload: { topicId: string, comment: Comment, content: string }): Promise<void> {
      const updateTopicComments = (topics: (MyTopic | ForeignTopic)[]) => {
        const topicIndex = topics.findIndex(t => t.id === payload.topicId);
        const topic = topics[topicIndex];
        if (topicIndex !== -1 && topic && topic.expanded) {
          if (payload.comment) {
            topic.comments.unshift(payload.comment);
          } else {
            this.refreshTopicComments(topic);
          }
        }
      };

      updateTopicComments(this.myTopics);
      updateTopicComments(this.foreignTopics);
    },
    async refreshTopicComments(topic: MyTopic | ForeignTopic): Promise<void> {
      if (topic.expanded) {
        topic.comments = await topicService.getComments(topic.id);
        topic.comments.sort((a: Comment, b: Comment) => {
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
      }
    },
  },
  computed: {
    mostLikedTopic() {
      if (!this.myTopics.length)
        return null;
      let result = this.myTopics[0]
      if (!result) return null;
      for (const topic of this.myTopics) {
        if (topic.votes > result.votes)
          result = topic;
      }
      return result;
    }
  },
  mounted() {
    this.fetchData()
  },
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

.topic {
  background-color: var(--color-background);
}

/* Remove left margin from headings to align with content */
.topic h1 {
  margin-left: 0;
}

.topic h2 {
  margin-left: 0;
}

/* Constrain instruction cards width */
.instructions-wrapper {
  max-width: 600px;
  margin-left: -0.25rem;
}

/* Desktop: Remove max-width constraint on instructions wrapper */
@media (min-width: 1200px) {
  .instructions-wrapper {
    max-width: none;
  }
}

/* Instructions and format explanation row */
.instructions-formats-row {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  margin-bottom: var(--space-xl);
  align-items: center;
}

/* Desktop: Side-by-side layout with balanced flex sizing */
@media (min-width: 1200px) {
  .instructions-formats-row {
    flex-direction: row;
    gap: var(--space-xl);
    align-items: flex-start;
  }
  
  .instructions-formats-row .instructions-wrapper {
    flex: 1 1 50%;
    max-width: none;
    min-width: 0;
  }
  
  /* TopicTypeStack gets remaining space */
  .instructions-formats-row > :last-child {
    flex: 1 1 50%;
    max-width: none;
    min-width: 0;
  }
}

/* Tablet: Stack with less gap */
@media (min-width: 785px) and (max-width: 1199px) {
  .instructions-formats-row {
    gap: var(--space-lg);
    padding-bottom: var(--space-xl);
  }
}

/* Sections container for side-by-side layout */
.sections-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl);
  max-width: 100%;
  overflow-x: hidden;
}

.topic-section {
  flex: 1;
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
}

/* Desktop: Keep sections stacked */
@media (min-width: 1200px) {
  .sections-container {
    flex-direction: column;
    gap: var(--space-xl);
  }
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

/* Info rows for presenter slot */
.info-row {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.info-label {
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
}

.info-value {
  font-size: var(--text-base);
  font-weight: var(--font-weight-medium);
  color: var(--color-primary);
}

/* Action button styling */
.action_button {
  cursor: pointer;
  background: none;
  border-style: none;
  border-radius: var(--radius-interactive);
  font-weight: bold;
  display: flex;
  place-items: center;
}

.topic_card_details_owner_actions {
  display: flex;
  flex-direction: row;
  align-content: center;
  align-items: center;
  gap: var(--space-sm);
}

.topic_card_details_owner_actions button {
  margin-left: var(--space-sm);
}

.my-topics-add-button-container {
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

</style>
<style scoped src="src/assets/instructions.css">
</style>

