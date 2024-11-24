<template>
  <header>
    <!-- <h1>{{ title }}</h1> -->
  </header>
  
  <Navbar />
  <Dashboard/>
  
  
</template>

<script setup>
import FileCreation from './components/FileCreation.vue';
import Dashboard from './components/Dashboard.vue';

import Navbar from './components/Navbar.vue' 
import { ref } from 'vue'

const title = ref('My First Vue App :)')
</script>

<style scoped>
#app{
  font-family: Avenir,Arial, Helvetica, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing:  grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;

}
header {
    line-height: 1.5;
    text-align: center;
    margin-bottom: 2rem;
    padding-top: 60px; /* Adjust based on the navbar height */
}

body {
    margin: 0; /* Ensure there's no margin */
    font-family: 'Nunito SemiBold';
    background: #eee;
}
</style>
