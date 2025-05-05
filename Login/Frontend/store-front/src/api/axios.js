
import axios from 'axios';

export default axios.create({
  baseURL: 'http://localhost:5117/api', 
  headers: {
    'Content-Type': 'application/json',
  },
});



