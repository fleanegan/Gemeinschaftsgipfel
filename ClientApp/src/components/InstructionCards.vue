<template>
  <div class="instruction_container">
    <div v-for="(instruction, index) in instructions" :key="index" class="instruction_card">
      <div class="instruction_card_content">
        <p class="instruction_card_content_header_title" style="padding-left: 2.25rem">{{ instruction.title }}</p>
        <p>{{ instruction.content }}</p>
      </div>
      <div class="instruction_card_enumerator">{{ index + 1 }}.</div>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent, type PropType} from 'vue';

export interface Instruction {
  title: string;
  content: string;
}

export default defineComponent({
  props: {
    instructions: {
      type: Array as PropType<Instruction[]>,
      required: true
    }
  }
});
</script>

<style scoped src="src/assets/instructions.css"></style>
