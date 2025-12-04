<template>
  <div class="topic wide-content">
    <h1>Helfende Hände</h1>
    <div class="instructions-wrapper">
      <p class="support_description">Freiwillige vor! Wir haben ein paar Aufgaben gesammelt, bei denen wir Hilfe brauchen.
        Mach
        mit und schaff die letzten Hürden auf dem Weg zum Gemeinschaftsgipfel aus dem Weg. Keine Scheu, hier steht das
        Vergnügen proportional zum Schweiß : Jeder Dienst wird in Dreiergruppen gestaltet, damit du auch bei diesem Teil
        des Festivals immer von netten Menschen umgeben bist. Zur vergeben sind (oh ja, du darfst dich auch mehrmals
        eintragen):</p>
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
import {scrollToTopMixin} from '@/mixins/scrollToTop';
import type {SupportTask} from '@/types/SupportTaskInterfaces';
import {supportTaskService} from '@/services/api';

export default defineComponent({
  components: {
    SupportTaskCard
  },
  mixins: [scrollToTopMixin],
  data() {
    return {
      supportTasks: [] as SupportTask[],
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

<style>
/* Override global .routed-elements width constraint */
.routed-elements:has(.wide-content) {
  width: 100% !important;
  max-width: 100% !important;
}
</style>

<style scoped>
.topic {
  background-color: var(--color-background);
}

/* Wide content for support task page */
.wide-content {
  width: 100%;
  padding: 0 2rem;
  max-width: 100%;
  overflow-x: hidden;
  box-sizing: border-box;
}

/* Small screens: Reduce horizontal padding */
@media (max-width: 400px) {
  .wide-content {
    padding: 0 0.5rem;
  }
}

/* Constrain instructions wrapper width */
.instructions-wrapper {
  max-width: 600px;
  margin-bottom: 2rem;
}

.support_description {
  margin: 0 0 1rem 0;
  line-height: 1.6;
}

/* Sections container for side-by-side layout */
.sections-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  max-width: 100%;
  overflow-x: hidden;
  margin-bottom: 2rem;
}

.support-section {
  flex: 1;
  min-width: 0;
  max-width: 100%;
  overflow-x: hidden;
}

/* Desktop: Sections side-by-side */
@media (min-width: 1200px) {
  .sections-container {
    flex-direction: row;
    gap: 2rem;
    align-items: flex-start;
  }
  
  .support-section {
    flex: 1 1 50%;
    min-width: 0;
  }
}

.list {
  background-color: var(--color-background-soft);
  padding: 1rem;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  list-style: none;
  margin: 1rem 0 0 0;
}

/* Small screens: Reduce list padding */
@media (max-width: 400px) {
  .list {
    padding: 0.5rem 0.5rem !important;
    gap: 0.25rem !important;
  }
}

.card_scroll_container {
  list-style: none;
}

h2 {
  margin-top: 0;
  margin-bottom: 0.75rem;
}

/* Summary section */
.summary-section {
  width: 100%;
  margin-top: 1rem;
  padding: 1rem;
  background-color: var(--color-background-soft);
  border-radius: 6px;
  box-sizing: border-box;
}

.summary-label {
  font-size: 0.6875rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  color: var(--color-main-text);
  opacity: 0.6;
  margin-bottom: 0.5rem;
  text-transform: uppercase;
}

.summary-content {
  font-size: 0.9375rem;
  color: var(--color-primary);
  line-height: 1.6;
  white-space: pre-wrap;
  background-color: var(--color-nuance-light);
  padding: 0.75rem;
  border-radius: 4px;
  margin: 0;
}

/* Small screens: Reduce summary padding */
@media (max-width: 400px) {
  .summary-section {
    padding: 0.75rem;
  }
}
</style>
<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css">
</style>
