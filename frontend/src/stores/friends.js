import { defineStore } from 'pinia'
import api from '@/api'

export const useFriendsStore = defineStore('friends', {
  state: () => ({
    friends: [],
    requests: [],
    requestsCount: 0
  }),
  actions: {
    async fetchFriends() {
      const { data } = await api.get('/friends')
      this.friends = data
    },
    async fetchRequests() {
      const { data } = await api.get('/friends/requests')
      this.requests = data
    },
    async fetchRequestsCount() {
      const { data } = await api.get('/friends/requests/count')
      this.requestsCount = data.count
    },
    async sendRequest(username) {
      await api.post('/friends/request', { username })
    },
    async accept(id) {
      await api.post(`/friends/requests/${id}/accept`)
      await this.fetchRequests()
      await this.fetchRequestsCount()
    },
    async decline(id) {
      await api.post(`/friends/requests/${id}/decline`)
      await this.fetchRequests()
      await this.fetchRequestsCount()
    },
    async remove(id) {
      await api.delete(`/friends/${id}`)
      await this.fetchFriends()
    }
  }
})