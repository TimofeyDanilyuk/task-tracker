import { createRouter, createWebHistory } from 'vue-router'
import BoardView from '@/views/BoardView.vue'
import TaskView from '@/views/TaskView.vue'
import TodoView from '@/views/TodoView.vue'
import AuthView from '@/views/AuthView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/auth', component: AuthView },
    { path: '/', component: BoardView, meta: { auth: true } },
    { path: '/task/:id', component: TaskView, meta: { auth: true } },
    { path: '/todo', component: TodoView, meta: { auth: true } }
  ]
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  if (to.meta.auth && !token) return { path: '/auth' }
  if (to.path === '/auth' && token) return { path: '/' }
})

export default router