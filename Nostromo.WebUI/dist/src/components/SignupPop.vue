<template>
  <div v-if="show" class="popup">
    <div class="popup-content">
      <button class="close-button" @click="close">x</button>
     
     
      <form>
        <p class="brand-name">Nostromos</p>
        <label>Email:</label>
        <input type="email" required v-model="email">
        <label>Password:</label>
        <input type="password" required v-model="password">
        <!-- <div class="remember"> 
          <label>Remember me</label>
          <input type="checkbox" v-model="remember">
          
        </div> -->
        
        <button class="submit-button">Log In </button>
        <p class="reset-password">Forgot your password?</p>
        <p class="sign-up">Not a member?  SIGN UP</p>

    </form>
    </div>
  </div>
</template>


<script>
import apiService from '@/apiService.js';
export default {
  props: {
    show: Boolean,
  },
  data() {
    return {
      isLoginVisible: true,
            email:' ',
            password:' ',
            remember: false

    };
  },
  methods: {
    close() {
      this.$emit('update:show', false);
    },
    sendInformation(){
       
    },
  },
};
</script>

<style scoped>
.popup {
  font-family: Arial, Helvetica, sans-serif;
  position: fixed;
  padding: top 20px;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background-color: white;
  width: 400px;
  height: 600px;
  color: #555;
  color: #555;
  transition: opacity 0.3s ease;
  z-index: 1000;
  display: flex;
  /* align-items: center; */
  justify-content: center;
  border-radius: 8px;
  padding: 20px;
  box-sizing: border-box;
}
.brand-name{
  font-size: 40px;
  font: bold;
  margin-bottom: 40px;
  text-align: center;
}
 

.popup-content {
  text-align: center;
  position: relative; /* Enables absolute positioning within this container */
  width: 100%;
}


.close-button {
  position: absolute;
  top: 10px;
  left: 10px;
  background: none;
  border: none;
  color: #333;
  font-size: 1.2rem;
  cursor: pointer;
}


form{
    max-width: 400px;
    max-height: 600px;
    margin: 30px auto;
    background: white;
    text-align: left;
    padding: 30px;
    top: 50%;
    left: 50%;
    border-radius: 10px;
}
label{
    color: #aaa;
    display: inline-block;
    margin: 25px 0 15px;
    font-size: .6em;
    text-transform: uppercase;
    letter-spacing: 1px;
    font-weight: bold;


}
input{
    display: block;
    padding: 10px 6px;
    width: 100%;
    border-radius: 10px;
    box-sizing: border-box;
    border: none;
    border-bottom: 1px solid #ddd;
    color: #555;

}
.close-button {
  display: flex;
  align-content: right;
  top: 10px;
  right: 10px;
  background: none;
  border: none;
  color: #333;
  font-size: 1.2rem;
  cursor: pointer;
}
.submit-button{
  cursor: pointer;
  align-items: center;
  border: none;
  border-radius: 10px;
  display: block;       /* Makes the button occupy full width of parent element */
  margin: 20px auto;
  width: 100%;
  height: 50px;

}
.reset-password{
  font-size: 12px;
  text-align: center;
}
.sign-up{
  font-size: 15px;
  text-align: center;
  padding-top: 20px;
}

</style>