<template>
  <div class="supporttask wide-content">
    <div :class="{'floating_scroll_to_top_hidden': true, 'floating_scroll_to_top_shown': isSticky}">
      <button class="action_button" style="margin-right: 1rem;" @click="scrollToTop">
        <img :src="'/expand.svg'" alt="Expand">
      </button>
    </div>
    <h1>Helfende Hände</h1>
    <div class="instructions-formats-row">
      <div class="instructions-wrapper">
        <InstructionCards :instructions="instructions" />
      </div>
    </div>
    
    <div class="sections-container">
      <!-- Left section: Gruppenaufgaben -->
      <section class="support-section">
        <h2>Gruppenaufgaben</h2>
        <p class="support_description">Hier zu helfen kostet nicht so viel Zeit und Energie, hilft aber ungemein</p>
        <ul class="list">
          <li v-for="(item, index) in groupSupportTasks" :key="index" class="card_scroll_container">
            <SupportTaskCard
              :task="item"
              :taskList="groupSupportTasks"
              :taskIndex="index"
              :toggleSupporting="toggleSupporting"
              :user-name="userName!"
              @show-supporter="item.showSupporter=true"
              @hide-supporter="item.showSupporter=false"
            />
          </li>
        </ul>
      </section>
      
      <!-- Right section: Hauptverantwortliche -->
      <section class="support-section">
        <h2><span style="font-weight: bold">NEU!</span> Hauptverantwortliche</h2>
        <p class="support_description">Hierfür braucht es etwas Engagement, und der Ruhm wird ewig währen.
          Zu Anfang des Festivals werden wir dich in Alles einweihen, was du wissen musst, damit du den spannenden Aufgaben
          problemlos Herr wirst und dann in der Lage bist, alle anderen Teilnehmer:innen in allen Fragen rund um deine Verantwortlichkeit zu unterstützen.
        </p>
        <ul class="list">
          <li v-for="(item, index) in singleSupportTasks" :key="index" class="card_scroll_container">
            <SupportTaskCard
              :task="item"
              :taskList="singleSupportTasks"
              :taskIndex="index"
              :toggleSupporting="toggleSupporting"
              :user-name="userName!"
              @show-supporter="item.showSupporter=true"
              @hide-supporter="item.showSupporter=false"
            />
          </li>
        </ul>
      </section>
    </div>
    
    <!-- Summary section: Subscribed tasks -->
    <div class="summary-section">
      <div class="summary-label">{{ allTheTaskNamesISubscribedTo.length ? "Da unterstützt du schon:" : "" }}</div>
      <div class="summary-content">
        {{ allTheTaskNamesISubscribedTo.length ? allTheTaskNamesISubscribedTo.join(", ") : "Oha, du hast dich noch nirgendwo eingetragen" }}
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { useAuthStore } from '@/store/auth';
import SupportTaskCard from '@/components/SupportTaskCard.vue';
import InstructionCards from '@/components/InstructionCards.vue';
import {scrollToTopMixin} from '@/mixins/scrollToTop';
import type {SupportTask} from '@/types/SupportTaskInterfaces';
import {supportTaskService} from '@/services/api';

export default defineComponent({
  components: {
    SupportTaskCard,
    InstructionCards
  },
  mixins: [scrollToTopMixin],
  data() {
    return {
      supportTasks: [] as SupportTask[],
      instructions: [
        {
          title: 'Aufgabe aussuchen',
          content: 'Freiwillige vor! Wir haben ein paar Aufgaben gesammelt, bei denen wir Hilfe brauchen. Keine Scheu, hier steht das Vergnügen proportional zum Schweiß.'
        },
        {
          title: 'Gemeinsam anpacken',
          content: 'Jeder Dienst wird in Dreiergruppen gestaltet, damit du auch bei diesem Teil des Festivals immer von netten Menschen umgeben bist.'
        },
        {
          title: 'Mehrfach helfen',
          content: 'Du darfst dich auch mehrmals eintragen und bei verschiedenen Aufgaben mithelfen. Jede helfende Hand wird gebraucht!'
        }
      ]
    };
  },
  methods: {
    useAuthStore,
    async fetchData() {
      this.supportTasks = (await supportTaskService.getAllTasks()).sort(function (a: SupportTask, b: SupportTask) {
        if (a.duration > b.duration)
          return 1;
        if (a.duration < b.duration)
          return -1;
        return 0;
      });
    },
    async toggleSupporting(tasks: SupportTask[], index: number): Promise<void> {
      const task = tasks[index];
      if (!task) return;
      if (task.supporterUserNames.includes(this.userName!)) {
        try {
          await supportTaskService.removeHelp(task.id);
        } catch (e: any) {
          console.error('Error removing help from support task:', e);
        }
        task.supporterUserNames = task.supporterUserNames.filter(x => x != this.userName!)
      } else {
        try {
          await supportTaskService.addHelp(task.id);
        } catch (e: any) {
          console.error('Error adding help to support task:', e);
        }
        task.supporterUserNames.push(this.userName!)
      }
    },
  },
  computed: {
    userName() {
      return this.useAuthStore().userName;
    },
    groupSupportTasks() {
      return this.supportTasks.filter((task: SupportTask) => task.requiredSupporters > 1);
    },
    singleSupportTasks() {
      return this.supportTasks.filter((task: SupportTask) => task.requiredSupporters === 1);
    },
    allTheTaskNamesISubscribedTo() {
      return this.supportTasks
          .filter((task: SupportTask) => task.supporterUserNames.includes(this.userName!))
          .map((task: SupportTask) => task.title)
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

.supporttask {
  background-color: var(--color-background);
}

/* Remove left margin from headings to align with content */
.supporttask h1 {
  margin-left: 0;
}

.supporttask h2 {
  margin-left: 0;
  font-size: var(--text-xl);
}

/* Instructions row (single column for SupportTaskView) */
.instructions-formats-row {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  margin-bottom: var(--space-xl);
  align-items: center;
}

/* Constrain instructions wrapper width */
.instructions-wrapper {
  max-width: 600px;
  margin-left: -0.25rem;
}

/* Sections container - always stacked vertically */
.sections-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl);
  max-width: 100%;
  overflow-x: hidden;
  margin-bottom: var(--space-xl);
}

.support-section {
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

.support-section {
  flex: 1;
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
}

.support-section h2 {
  margin-top: 0;
  margin-bottom: var(--space-sm);
}

.support-section .support_description {
  margin: 0 0 var(--space-md) 0;
  line-height: 1.6;
  font-size: var(--text-sm);
}

.list {
  background-color: var(--color-background-soft);
  padding: var(--space-md);
  border-radius: var(--radius-sharp);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  list-style: none;
  margin: var(--space-md) 0 0 0;
}

/* Small screens: Reduce list padding */
@media (max-width: var(--breakpoint-mobile)) {
  .list {
    padding: var(--space-sm) var(--space-sm) !important;
    gap: var(--space-xs) !important;
  }
}

.card_scroll_container {
  list-style: none;
}

/* Summary section */
.summary-section {
  width: 100%;
  margin-top: var(--space-xl);
  padding: var(--space-md);
  background-color: var(--color-background-soft);
  border-radius: var(--radius-sharp);
  box-sizing: border-box;
}

.summary-label {
  font-size: var(--text-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
  margin-bottom: var(--space-sm);
  text-transform: uppercase;
}

.summary-content {
  font-size: var(--text-base);
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding: var(--space-sm);
  border-radius: var(--radius-interactive);
  margin: 0;
}

/* Small screens: Reduce summary padding */
@media (max-width: var(--breakpoint-mobile)) {
  .summary-section {
    padding: var(--space-sm);
  }
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
</style>
<style scoped src="src/assets/instructions.css"></style>
