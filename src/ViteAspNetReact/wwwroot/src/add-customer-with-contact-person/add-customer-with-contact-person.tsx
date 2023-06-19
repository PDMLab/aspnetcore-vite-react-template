import {useForm} from "react-hook-form";

export default function TestComponent() {
  const {
    register,
    handleSubmit,
    formState: {errors},
  } = useForm();
  const onSubmit = (data) => console.log(data);

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label htmlFor="name">A</label>
        <input
          type="text"
          id="name"
          className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
          {...register("name", {required: true, maxLength: 5})}
        />
        {errors.name && errors.name.type === "required" && (
          <span>This is required</span>
        )}
        {errors.name && errors.name.type === "maxLength" && (
          <span>Max length exceeded</span>
        )}</div>
      <NestedComponent1 register={register}/>
      <NestedComponent2 register={register}/>
      <button type="submit">Add</button>
    </form>
  );
}

function NestedComponent1({register}) {
  return (
    <div style={{display: "flex", flexDirection: "column"}}>
      <label htmlFor="nested-comp1-name1">B</label>
      <input type="text"
             className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
             {...register("nested-comp1-name1")} />
      <label htmlFor="nested-comp1-name2">C</label>
      <input type="text"
             className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
             {...register("nested-comp1-name2")} />
      <label htmlFor="nested-comp1-name3">D</label>
      <input type="text"
             className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
             {...register("nested-comp1-name3")} />
    </div>
  );
}

function NestedComponent2({register}) {
  return (
    <div style={{display: "flex", flexDirection: "column"}}>
      <label htmlFor="nested-comp2-name1">E</label>
      <input
        type="text"
        className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
        {...register("nested-comp2-name1")}
      />
      <label htmlFor="nested-comp2-name2">F</label>
      <input
        type="text"
        className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
        {...register("nested-comp2-name2")}
      />
      <label htmlFor="nested-comp2-name3">G</label>
      <input
        type="text"
        className={"border-gray-300 border rounded-md focus:ring focus:ring-indigo-200 focus:ring-opacity-50 p-2"}
        {...register("nested-comp2-name3")}
      />
    </div>
  );
}
