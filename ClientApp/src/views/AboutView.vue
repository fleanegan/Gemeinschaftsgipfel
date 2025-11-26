<template>
  <h1>Template</h1>
  <button @click="callRejector()">get rejected</button>
</template>


<script lang="ts">
import {defineComponent} from 'vue';
import {authService} from '@/services/api';

export default defineComponent({
  data() {
    return {};
  },
  methods: {
    async callRejector() {
      await authService.rejectMe();
    }
  },
  computed: {}
});
</script>

