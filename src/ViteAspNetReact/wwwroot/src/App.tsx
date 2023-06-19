import {useEffect, useState} from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import './App.css'
import {AddCustomer} from "./add-customer/add-customer.tsx";
import TestComponent from "./add-customer-with-contact-person/add-customer-with-contact-person.tsx";

function App() {
  const [count, setCount] = useState(0)
  const [greeting, setGreeting] = useState('');

  let initialized = false;

  useEffect(() => {
    if (!initialized) {
      initialized = true
      const fetchData = async () => {
        const response = await fetch('api/hello');
        const data = await response.json() as { hello: string };
        console.log(data);
        setGreeting(data.hello);
      };
      fetchData();
    }
  }, []);


  return (
    <>
      <div>
        <a href="https://vitejs.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo"/>
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo"/>
        </a>
      </div>
      <h1>Vite + React + ASP.NET Core</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <div>
        <p>hello, <span className={'text-green-400'}>{greeting}</span></p>
      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
      <AddCustomer></AddCustomer>

      <TestComponent></TestComponent>
    </>
  )
}

export default App
