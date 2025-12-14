<template>
  <global-loading/>
  <header v-if="!isLoginPage" :class="{'nav_header': true, 'nav_header_sticky': isSticky && isStandardPage && !isMenuOpen}">
    <nav class="nav-links">
      <router-link v-if="isStandardPage" class="router-link logo-link" to="/"><img alt="Home" src="/icon.svg"
                                                                         style="width: 6rem; height: 6rem; max-height: 4rem; max-width: 4rem;">
      </router-link>
      <div class="transparent-header-area"></div>
      <div class="desktop-links">
        <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/topic">Inhalte
        </router-link>
        <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/supporttask">Mithelfen
        </router-link>
        <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/rideshare">Fahrgemeinschaften
        </router-link>
        <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/forum">Forum
        </router-link>
        <router-link :class="{'router-link': true, 'headerless': !isStandardPage}" to="/schedule">Ablaufplan
        </router-link>
      </div>
      <button class="burger-menu" :class="{'menu-hidden': isMenuOpen}" @click="toggleMenu" aria-label="Menu">
        <span></span>
        <span></span>
        <span></span>
      </button>
    </nav>
  </header>
  
  <!-- Mobile Menu Overlay -->
  <transition name="menu-fade">
    <div v-if="isMenuOpen" class="mobile-menu-overlay">
      <button class="close-menu" @click="toggleMenu" aria-label="Close menu">
        <span></span>
        <span></span>
      </button>
      <nav class="mobile-menu-links">
        <router-link to="/" @click="toggleMenu">Home</router-link>
        <router-link to="/topic" @click="toggleMenu">Inhalte</router-link>
        <router-link to="/supporttask" @click="toggleMenu">Mithelfen</router-link>
        <router-link to="/rideshare" @click="toggleMenu">Fahrgemeinschaften</router-link>
        <router-link to="/forum" @click="toggleMenu">Forum</router-link>
        <router-link to="/schedule" @click="toggleMenu">Ablaufplan</router-link>
      </nav>
    </div>
  </transition>
  
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
      backgroundGradient: '',
      isMenuOpen: false
    };
  },
  computed: {
    isLoginPage() {
      return this.$route.path === '/login';
    }
  },
  methods: {
    handleScroll: function () {
      this.isSticky = window.scrollY > 0;
    },
    updateBackgroundGradient() {
      const contentHeight = document.documentElement.scrollHeight + 'px';
      this.backgroundGradient = `linear-gradient(to bottom, #ffffff, #000000) 0% 0% / 100% ${contentHeight}`;
    },
    toggleMenu() {
      this.isMenuOpen = !this.isMenuOpen;
      // Prevent body scroll when menu is open
      if (this.isMenuOpen) {
        document.body.classList.add('menu-open');
        document.documentElement.classList.add('menu-open');
      } else {
        document.body.classList.remove('menu-open');
        document.documentElement.classList.remove('menu-open');
      }
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
