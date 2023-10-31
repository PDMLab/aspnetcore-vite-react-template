namespace ViteAspNetReact.Features.RegisterEmployee;

public record EmployeeRegistered(
  string FirstName,
  string LastName,
  string Sub,
  string Email
);

public class Employee
{
  public Guid Id { get; set; }
  public string Firstname { get; }
  public string Lastname { get; }
  public string Email { get; }
  public string Sub { get; }

  private Employee(
    string firstname,
    string lastname,
    string sub,
    string email
  )
  {
    Firstname = firstname;
    Lastname = lastname;
    Sub = sub;
    Email = email;
  }

  public static Employee Create(
    EmployeeRegistered employeeRegistered
  )
  {
    var (firstName, lastName, sub, email) = employeeRegistered;
    return new Employee(
      firstName,
      lastName,
      sub,
      email
    );
  }
}
