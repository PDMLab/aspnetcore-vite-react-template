function App() {
  return (
    <div className={'flex flex-col h-screen justify-center items-center'}>
      <h1 className={'text-6xl'}>Vite + React + ASP.NET Core</h1>
      <button
        className={'mt-4 rounded-md bg-blue-500 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-blue-400 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-500'}
        onClick={async () => {
          await fetch('/api/signout', {
            method: 'POST'
          })
          window.location.reload()
        }
        }>
        Abmelden
      </button>
    </div>
  )
}

export default App
