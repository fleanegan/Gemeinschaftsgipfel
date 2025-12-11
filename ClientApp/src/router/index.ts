// router/index.ts
import {createRouter, createWebHistory} from 'vue-router';
import HomeView from '../views/HomeView.vue';
import {useAuthStore} from '@/store/auth';

const router = createRouter({
    history: createWebHistory("/"),
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition;
        } else {
            return { top: 0 };
        }
    },
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomeView,
            meta: {requiresAuth: true}
        },
        {
            path: '/about',
            name: 'about',
            component: () => import('../views/AboutView.vue'),
            meta: {requiresAuth: false}
        },
        {
            path: '/login',
            name: 'login',
            component: () => import('../views/LoginView.vue'),
            meta: {requiresAuth: false}
        }, {
            path: '/topic',
            name: 'Inhalte',
            component: () => import('../views/TopicView.vue'),
        },
        {
            path: '/schedule',
            name: 'Ablaufplan',
            component: () => import('../views/ScheduleView.vue'),
        },
        {
            path: '/supporttask',
            name: 'HelfendeHaende',
            component: () => import('../views/SupportTaskView.vue'),
        },
        {
            path: '/topic/add',
            name: 'Neues Thema hinzufügen',
            component: () => import('../views/InputTopicView.vue'),
            props: false
        },
        {
            path: '/topic/edit:topicId',
            name: 'Thema bearbeiten',
            component: () => import('../views/InputTopicView.vue'),
            props: true
        },
        {
            path: '/rideshare',
            name: 'Fahrgemeinschaften',
            component: () => import('../views/RideShareView.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/rideshare/add',
            name: 'Fahrt anbieten',
            component: () => import('../views/InputRideShareView.vue'),
            props: false,
            meta: { requiresAuth: true }
        },
        {
            path: '/rideshare/edit/:rideShareId',
            name: 'Fahrt bearbeiten',
            component: () => import('../views/InputRideShareView.vue'),
            props: true,
            meta: { requiresAuth: true }
        },
        {
            path: '/forum',
            name: 'Forum',
            component: () => import('../views/ForumView.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/forum/add',
            name: 'Neuer Forumsbeitrag',
            component: () => import('../views/InputForumView.vue'),
            props: false,
            meta: { requiresAuth: true }
        },
        {
            path: '/forum/edit/:forumId',
            name: 'Forumsbeitrag bearbeiten',
            component: () => import('../views/InputForumView.vue'),
            props: true,
            meta: { requiresAuth: true }
        }
    ]
});

router.beforeEach((to, from, next) => {
    const authStore = useAuthStore(); // Move this inside the beforeEach guard
    if (to.meta.requiresAuth !== false && !authStore.token) {
        next({
                path: '/login',
                query: {redirect: to.fullPath}
            }
        );
    } else {
        // Continue navigation
        next();
    }
});

export default router;
