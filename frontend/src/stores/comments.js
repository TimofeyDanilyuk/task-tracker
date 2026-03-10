import { defineStore } from 'pinia'
import api from '@/api'

export const useCommentsStore = defineStore('comments', {
  state: () => ({ comments: [] }),
  actions: {
    async fetch(taskId) {
      const { data } = await api.get(`/comments/task/${taskId}`)
      this.comments = data
    },
    async create(comment) {
      const { data } = await api.post('/comments', comment)
      this.comments.push(data)
    },
    async remove(id) {
      await api.delete(`/comments/${id}`)
      this.comments = this.comments.filter(c => c.id !== id)
    }
  }
})