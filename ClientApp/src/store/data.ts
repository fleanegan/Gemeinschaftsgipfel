import {defineStore} from 'pinia';

export const useDataStore = defineStore('data', {
    state: () => ({
	isLoading: false,
    }),
    actions: {
	startLoading() {
	    this.isLoading = true;
	},
	stopLoading() {
	    this.isLoading = false;
	},
    },
});
