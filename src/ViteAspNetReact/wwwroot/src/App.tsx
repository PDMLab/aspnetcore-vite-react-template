import './App.css'

function App() {
  return (
    <>
      <h1>Vite + React + ASP.NET Core</h1>
      <button className={'border-2 border-blue-500 text-sm hover:border-blue-400 mt-10'} onClick={async () => {
        await fetch('/api/signout', {
          method: 'POST'
        })
        window.location.reload()
      }
      }>
        Abmelden
      </button>
    </>
  )
}

export default App
