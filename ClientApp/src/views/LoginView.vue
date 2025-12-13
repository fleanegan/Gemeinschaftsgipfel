<template>
  <div class="login-page-wrapper">
    <div class="login-container">
      <h1>Kennen wir uns?</h1>
      <div class="checkbox-container">
      <p :class="{active: !isSignup, inactive: isSignup}">Anmelden</p>
      <label class="switch"><input v-model="isSignup" type="checkbox" @click="errors=''"/>
        <div></div>
      </label>
      <p :class="{active: isSignup, inactive: !isSignup}">Neu hier ! </p>
    </div>
    <form class="login-form" @submit.prevent="submitData">
      <div class="form-group">
        <label for="username">{{ isSignup ? "Neuer Benutzername" : "Benutzername" }}</label>
        <input id="username" v-model="username" class="form-input" type="text"/>
      </div>
      <div class="form-group">
        <label for="password">{{ isSignup ? "Neues " : "" }}Passwort</label>
        <input id="password" v-model="password" class="form-input" type="password"/>
      </div>
      <div v-if="isSignup" class="form-group">
        <label for="passwordConfirmation">Passwort bestätigen</label>
        <input id="passwordConfirmation" v-model="passwordConfirmation" class="form-input" type="password"/>
      </div>
      <p v-if="isSignup && !passwordsMatching" class="errors">Die Passwörter stimmen nicht überein.</p>
      <div v-if="isSignup" class="form-group">
        <label for="entrySecret">Eintrittsgeheimnis</label>
        <input id="entrySecret" v-model="entrySecret" class="form-input" type="password"/>
      </div>
      <textarea v-if="errors!=''" class="errors" inputmode="none">{{ errors }}</textarea>
      <button class="submit-button" type="submit">Abschicken</button>
    </form>
    </div>
    <div class="impressum" v-html="impressum"></div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import {useAuthStore} from '@/store/auth';
import {AxiosError} from "axios";
import {authService, homeService} from '@/services/api';

interface ErrorResponseData {
  description: String
}

export default defineComponent({
      data() {
        return {
          username: '',
          password: '',
          passwordConfirmation: '',
          entrySecret: '',
          errors: '',
          isSignup: false,
	  impressum: '',
        };
      },
      computed: {
        passwordsMatching: function passwordsMatching() {
          return this.password == this.passwordConfirmation;
        }
      },
      async mounted() {
        this.impressum = await homeService.getImpressum();
      },
      methods: {
        async submitData() {
          if (this.isSignup)
            await this.handleSignup()
          else
            await this.handleLogin()
        },
        async handleLogin() {
          try {
            const token = await authService.login({
              username: this.username,
              password: this.password
            });

            useAuthStore().login(token, this.username);
            let routeToPush = {path: '/'};
            if (this.$route.query.redirect) {
              routeToPush = {path: this.$route.query.redirect as string};
            }
            this.$router.push(routeToPush);
          } catch (e) {
            const r = e as AxiosError;
            if (r.response?.status == 401)
              this.errors = "Falsches Passwort oder nicht existierender Nutzername"
            else if (r.response?.status == 429)
              this.errors = "Zu viele Versuche. Bitte in 60s noch erneut probieren."
          }
        }
        , async handleSignup() {
          try {
            await authService.signup({
              username: this.username,
              password: this.password,
              entrySecret: this.entrySecret,
            });
            await this.handleLogin();
          } catch (e: any) {
            if (e.response?.status == 429) {
              this.errors = "Zu viele Versuche. Bitte in 60s noch erneut probieren."
              return;
            }
            const responseData = e.response?.data;
            if (Array.isArray(responseData)) {
              this.errors = responseData
                .map(error => error.description || error.Description || JSON.stringify(error))
                .join("\n");
            } else if (responseData && typeof responseData === 'object') {
              this.errors = responseData.description || responseData.Description || JSON.stringify(responseData);
            } else {
              this.errors = "Ein Fehler ist aufgetreten. Bitte versuche es erneut.";
            }
          }
        }
      },
    },
)
</script>

<style scoped>

.login-page-wrapper {
  width: 100%;
  max-width: 100vw;
  overflow-x: hidden;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

.login-container {
  max-width: 600px;
  width: 100%;
  margin: 0 auto;
  padding: 0 1rem;
  box-sizing: border-box;
}

.login-container h1 {
  text-align: left;
  margin-left: 0;
  font-size: 1.5rem;
}

.checkbox-container {
  text-align: center;
  display: flex;
  place-content: center;
  align-content: center;
  align-items: center;
  flex-direction: row;
  margin: 2rem auto 2rem;
}

.switch input {
  position: absolute;
  opacity: 0;
}

.switch {
  display: inline-flex;
  align-items: center;
  font-size: 1rem;
  height: 1.4rem;
  width: 2.4rem;
  border-radius: 1rem;
  border-color: var(--color-main-text);
  border-style: solid;
  border-width: 0.29rem;
  position: relative;
}

.switch div {
  height: 1rem;
  width: 1rem;
  border-radius: 1rem;
  background-color: var(--color-main-text);
  -webkit-transition: all 300ms;
  -moz-transition: all 300ms;
  transition: all 300ms;
}

.switch input:checked + div {
  -webkit-transform: translate3d(100%, 0, 0);
  -moz-transform: translate3d(100%, 0, 0);
  transform: translate3d(100%, 0, 0);
}

.checkbox-container p {
  font-size: 0.85rem;
  width: 5rem;
  margin-right: 1rem;
}

.active {
  color: var(--color-main-text);
}

.inactive {
  color: #adb5bd;
}

.login-form {
  display: flex;
  flex-direction: column;
}

.form-group {
  display: flex;
  flex-direction: column;
  padding: 0.5rem 0;
}

.errors {
  color: red;
  margin-left: 0;
  margin-right: 0;
  font-size: 0.75rem;
  resize: none;
  border: none;
}

.submit-button {
  margin-left: auto;
  margin-right: 0;
  margin-top: 1rem;
}

.impressum{
  position: absolute; 
  top: 100vh;
  padding: 0 1rem;
  box-sizing: border-box;
  width: 100%;
  max-width: 100vw;
  overflow-x: hidden;
}

.impressum p{
  margin-left: 0;
  max-width: 100%;
  word-wrap: break-word;
  overflow-wrap: break-word;
  box-sizing: border-box;
}

:deep(p) {
  margin-left: 0;
  margin-right: 0;
  word-wrap: break-word;
  overflow-wrap: break-word;
  max-width: 100%;
}

@media (max-width: 600px) {
  .login-container h1 {
    font-size: 1.25rem;
  }
  
  .checkbox-container {
    margin: 1.5rem auto 1.5rem;
  }
  
  .checkbox-container p {
    font-size: 0.75rem;
    width: 4rem;
    margin-right: 0.5rem;
  }
  
  .form-group {
    padding: 0.25rem 0;
  }
  
  .form-group label {
    font-size: 0.9rem;
  }
  
  .form-input {
    font-size: 0.9rem;
  }
}
</style>
