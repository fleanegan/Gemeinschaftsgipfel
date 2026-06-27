<template>
  <div class="topic">
    <h1>Ablaufplan</h1>
    <img :src="'/schedule.svg'" alt="schedule" style="width: 100%; max-width: var(--breakpoint-tablet);">
  </div>
</template>


<script lang="ts">
import {defineComponent} from 'vue';
import axios from "axios";
import {useAuthStore} from '@/store/auth';


export default defineComponent({});
</script>

<style scoped>
</style>
<style scoped src="src/assets/topics.css"></style>
<style scoped src="src/assets/instructions.css">
</style>
