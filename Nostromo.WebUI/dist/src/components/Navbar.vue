<template>
    <div class="navbar">
      <ul class="navbar-nav">
        <li class="nav-item">
          <a class="nav-link active" aria-current="page" href="#">Home</a>
        </li>
        <li class="nav-item">
          <a class="nav-link" href="#">Dashboard</a>
        </li>
        <li class="nav-item">
          <a class="nav-link" href="/collection">Collection</a>
        </li>
      </ul>
      <div class="nav-item">
        <button class="login-button" @click="toggleLogin">Login</button>
        <SignupPop v-model:show="isLoginVisible"></SignupPop>
      </div>
    </div>
  </template>
  
  <script>
  import SignupPop from './SignupPop.vue';
  import FileCreation from './FileCreation.vue';
  
  export default {
    components: {
      SignupPop
    },
    data() {
      return {
        isLoginVisible: false,
            email:' ',
            password:' '
      }
    },
    methods: {
      toggleLogin() {
        this.isLoginVisible = !this.isLoginVisible;
      }
    }
  }
  </script>
  
  <style scoped>
  .navbar {
    top: 0;
    width: 100%;
    height: 100px;
    background-color: white;
    z-index: 1000;
    align-items: center;
    position: fixed;
    display: flex;
    left: 0;
    justify-content: space-between; /* Add this line */
  }
  
  .navbar-nav {
    display: flex;
    gap: 20px;
    list-style: none;
    padding: 0;
    margin: 0;
  }
  
  .navbar-nav .nav-link {
    color: #000; /* Change text color to ensure visibility */
    padding: 10px 15px;
    text-decoration: none;
  }
  
  .login-button {
    color: white;
    margin-left: -90px;
    text-decoration: none;
    border: none;
    background-color: transparent; /* Add background color if needed */
    cursor: pointer; /* Change cursor to pointer for better UX */
  }
  </style>
  