import { defineStore } from 'pinia'
import api from '@/api'

export const useTasksStore = defineStore('tasks', {
  state: () => ({ tasks: [], current: null }),
  actions: {
    async fetch(isTodo = false) {
      const { data } = await api.get('/tasks', { params: { isTodo } })
      this.tasks = data
    },
    async fetchOne(id) {
      const { data } = await api.get(`/tasks/${id}`)
      this.current = data
    },
    async create(task) {
      const { data } = await api.post('/tasks', task)
      this.tasks.push(data)
      return data
    },
    async update(id, task) {
      const { data } = await api.put(`/tasks/${id}`, task)
      const i = this.tasks.findIndex(t => t.id === id)
      if (i !== -1) this.tasks[i] = data
    },
    async changeStage(id, stageId) {
      await api.patch(`/tasks/${id}/stage`, { stageId })
      const task = this.tasks.find(t => t.id === id)
      if (task) task.stageId = stageId
    },
    async toggleDone(id) {
      const { data } = await api.patch(`/tasks/${id}/done`)
      const task = this.tasks.find(t => t.id === id)
      if (task) task.isDone = data.isDone
    },
    async remove(id) {
      await api.delete(`/tasks/${id}`)
      this.tasks = this.tasks.filter(t => t.id !== id)
    }
  }
})