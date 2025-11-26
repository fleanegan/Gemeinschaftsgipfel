<template>
  <div class="topic">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Zeig uns was 'ne Harke ist!</h1>
    <InstructionCards :instructions="instructions" />
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
        <template #actions>
          <div class="topic_card_details_owner_actions">
            <button class="action_button" style="margin-bottom: 0.25rem;" @click="removeTopic(item.id)">
              <img alt="Delete" src="/empty_delete.svg">
            </button>
            <button class="action_button" @click="editTopic(item.id)">
              <img alt="Edit" src="/empty_edit_no_border.svg">
            </button>
          </div>
        </template>
      </TopicCard>
      <li>
        <hr>
        <div id="owner_action" class="my-topics-add-button-container">
          <button class="submit-button" @click="addNewTopic">Neue Idee?</button>
        </div>
      </li>
    </ul>
    <h2>Ideen der Anderen</h2>
    <ul class="list">
      <TopicCard
          v-for="(item, index) in foreignTopics"
          :key="item.id"
          :topic="item"
          :isHighlighted="false"
          @toggle-details="toggleDetails(foreignTopics, index)"
          @comment-sent="handleCommentSent"
      >
        <template #action-button>
          <button class="action_button" @click="toggleVote(index)">
            <img :src="item.didIVoteForThis ? '/full_heart.svg' : '/empty_heart.svg'" alt="Vote">
          </button>
        </template>
        <template #presenter>
          <p class="presenter">{{ item.presenterUserName }}</p>
        </template>
      </TopicCard>
      <li class="topic-card-details">Ende der Liste. Danke fürs Abstimmen!
        <br style="margin-bottom: 25rem">
      </li>
    </ul>
  </div>
</template>


<script lang="ts">
import {defineComponent} from 'vue';
import type {ForeignTopic, MyTopic, Comment} from "@/types/TopicInterfaces";
import TopicCard from "@/components/TopicCard.vue";
import {formatDateTime} from '@/utils/dateFormatter';
import {scrollToTopMixin} from '@/mixins/scrollToTop';
import InstructionCards from '@/components/InstructionCards.vue';
import {topicService} from '@/services/api';


export default defineComponent({
  components: {TopicCard, InstructionCards},
  mixins: [scrollToTopMixin],
  data() {
    return {
      foreignTopics: [] as ForeignTopic[],
      myTopics: [] as MyTopic[],
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
    async removeTopic(topicId: string) {
      await topicService.deleteTopic(topicId);
      await this.fetchData();
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

</style>
<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css">
</style>

