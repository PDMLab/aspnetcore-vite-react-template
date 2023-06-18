import {useForm, SubmitHandler,} from "react-hook-form"

export type Customer = {
  companyName: string
}


type Inputs = Customer

export const AddCustomer = () => {

  const {
    register,
    handleSubmit,
    setError,
    formState: {errors},
  } = useForm<Inputs>()

  const onSubmit: SubmitHandler<Inputs> = (data) => {
    fetch('api/customer', {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify(data)
    }).then((response =>
      response.json()
        .then(data => {
          if(!response.ok) {
            console.log('error', data);
          setError('companyName', {type: 'custom', message: 'Company name already exists'});
          }
        })
        .catch((error) => {
          console.log('error');
          setError('companyName', {type: 'custom', message: 'Company name already exists'});
        })));
  }


  return (<div>
    <h2 className='font-bold text-lg'>Add Customer</h2>
    <form onSubmit={handleSubmit(onSubmit)}>
      <input className={'bg-green-200 text-green-700'} {...register("companyName")} />
      {errors.companyName && <div className='bg-red-500'>This field is required</div>}
    </form>
  </div>)
}
