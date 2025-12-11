/**
 * Preload topic card photos eagerly to ensure they're cached before user navigates to topic view
 * Uses the Image() constructor pattern similar to TopicTypeExplanationStack.vue
 */

const TOPIC_PHOTOS = [
  '/photos/workshop.jpg',
  '/photos/presentation.png',
  '/photos/sport.jpg',
  '/photos/discussion.jpg',
  '/photos/misc.jpg',
];

export function preloadTopicPhotos(): void {
  TOPIC_PHOTOS.forEach((photoUrl) => {
    const img = new Image();
    img.onload = () => {
      // Image successfully preloaded and cached by browser
      console.debug(`Preloaded: ${photoUrl}`);
    };
    img.onerror = () => {
      // Image failed to load - log warning but don't block
      console.warn(`Failed to preload image: ${photoUrl}`);
    };
    img.src = photoUrl;
  });
}
