<template>
  <h1>{{ isEditing ? 'Fahrt bearbeiten' : 'Fahrt anbieten' }}</h1>
  <form @submit.prevent="submitData">
    <div class="form-group">
      <label for="from">Von (Startort)</label>
      <input id="from" v-model="from" class="form-input" type="text" required/>
    </div>
    <div class="form-group">
      <label for="to">Nach (Zielort)</label>
      <input id="to" v-model="to" class="form-input" type="text" required/>
    </div>
    <div class="form-group">
      <label for="departureTime">Abfahrtszeit</label>
      <input id="departureTime" v-model="departureTime" class="form-input" type="datetime-local" required/>
    </div>
    <div class="form-group">
      <label for="availableSeats">Verfügbare Plätze</label>
      <input id="availableSeats" v-model.number="availableSeats" class="form-input" type="number" min="1" required/>
    </div>
    <div class="form-group">
      <label for="stops">Zwischenstopps (optional)</label>
      <textarea id="stops" v-model="stops" class="form-input" rows="2"></textarea>
    </div>
    <div class="form-group">
      <label for="description">Beschreibung (optional)</label>
      <textarea id="description" v-model="description" class="form-input" rows="4"></textarea>
    </div>
    <div class="button-container">
      <button class="abort-button" type="button" @click="abort">Verwerfen</button>
      <button class="submit-button" type="submit">Abschicken</button>
    </div>
  </form>
</template>

<script lang="ts">
import {defineComponent} from 'vue';
import {rideShareService} from '@/services/api';

export default defineComponent({
  data() {
    return {
      from: '',
      to: '',
      departureTime: '',
      availableSeats: 1,
      stops: '',
      description: '',
    };
  },
  methods: {
    isRideShareIdSet(): boolean {
      return this.$props['rideShareId'] !== undefined && this.$props['rideShareId'] !== null;
    },
    async submitData() {
      try {
        if (this.isEditing) {
          await rideShareService.updateRideShare({
            id: this.$props["rideShareId"],
            from: this.from,
            to: this.to,
            departureTime: this.departureTime,
            availableSeats: this.availableSeats,
            stops: this.stops || undefined,
            description: this.description || undefined,
          });
        } else {
          await rideShareService.createRideShare({
            from: this.from,
            to: this.to,
            departureTime: this.departureTime,
            availableSeats: this.availableSeats,
            stops: this.stops || undefined,
            description: this.description || undefined,
          });
        }

        this.$router.push('/rideshare');
      } catch (e) {
        console.error('Error submitting rideshare:', e);
      }
    },
    async abort() {
      this.$router.push('/rideshare');
    }
  },
  computed: {
    isEditing() {
      return this.isRideShareIdSet();
    },
  },
  async beforeCreate() {
    if (this.$props['rideShareId'] !== undefined && this.$props['rideShareId'] !== null) {
      try {
        const existingRideShare = await rideShareService.getRideShare(this.$props["rideShareId"]);
        this.from = existingRideShare["from"];
        this.to = existingRideShare["to"];
        this.departureTime = existingRideShare["departureTime"];
        this.availableSeats = existingRideShare["availableSeats"];
        this.stops = existingRideShare["stops"] || '';
        this.description = existingRideShare["description"] || '';
      } catch (e) {
        console.log("edit: could not get existing rideshare")
      }
    }
  },
  props: ['rideShareId'],
});
</script>

<style scoped>
.form-group {
  display: flex;
  flex-direction: column;
  padding: 0 1rem 1rem 1rem;
}

.title-error {
  color: red;
}

.button-container {
  display: flex;
  flex-direction: row;
}

.submit-button {
  margin-left: auto;
  margin-right: 1rem;
}

.form-input {
  padding: 0.5rem;
  border: 1px solid var(--main-color-primary);
  border-radius: 0.25rem;
  font-size: 1rem;
}

textarea.form-input {
  resize: vertical;
  font-family: inherit;
}
</style>
