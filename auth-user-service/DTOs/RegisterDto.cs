namespace auth_user_service.DTOs
{
    public class RegisterDto
    {

            public string Name { get; set; }        
            public string Surname { get; set; }    
            public string Email { get; set; }       
            public string PhoneNumber { get; set; } 
            public string Password { get; set; }    
            public List<string> Roles { get; set; } 
       


    }
}
