<template>
  <global-loading/>
  <header :class="{'nav_header': true, 'nav_header_sticky': isSticky && isStandardPage}">
    <nav class="nav-links">
      <router-link v-if="isStandardPage" class="router-link" to="/"><img alt="Home" src="/icon.svg"
                                                                         style="width: 6rem; height: 6rem; max-height: 48px; max-width: 48px;">
      </router-link>
      <div class="transparent-header-area"></div>
      <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/topic">Inhalte
      </router-link>
      <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/supporttask">Mithelfen
      </router-link>
      <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/rideshare">Fahrgemeinschaften
      </router-link>
      <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/schedule">Ablaufplan
      </router-link>
    </nav>
  </header>
  <div :class="{'routed-elements': isStandardPage, 'home_page_routed_elements': !isStandardPage}">
    <router-view/>
  </div>
</template>

<script lang="ts">
import {defineComponent, watch} from 'vue';
import {useRoute} from "vue-router";

export default defineComponent({
  data() {
    return {
      isSticky: false,
      isStandardPage: true,
      backgroundGradient: ''
    };
  },
  methods: {
    handleScroll: function () {
      this.isSticky = window.scrollY > 0;
    },
    updateBackgroundGradient() {
      const contentHeight = document.documentElement.scrollHeight + 'px';
      this.backgroundGradient = `linear-gradient(to bottom, #ffffff, #000000) 0% 0% / 100% ${contentHeight}`;
    }
  },
  created() {
    const currentRoute = useRoute();
    this.isStandardPage = currentRoute.path !== '/';
    watch(() => currentRoute.path, (newPath) => {
      this.isStandardPage = newPath !== '/';
    });
  },
  mounted() {
    window.addEventListener('scroll', this.handleScroll);
    window.addEventListener('resize', this.updateBackgroundGradient);
    // Set initial scroll state
    this.handleScroll();
  },
  beforeUnmount() {
    window.removeEventListener('scroll', this.handleScroll);
    window.removeEventListener('resize', this.updateBackgroundGradient);
  },
});
</script>

<style src="./assets/header.css"></style>
