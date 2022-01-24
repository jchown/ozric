using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OzricEngine;

namespace OzricService.Pages;

public class GraphModel : PageModel
{
    [BindProperty]
    public string GraphJSON { get; set; }
    
    public IActionResult OnGet()
    {
        GraphJSON = Json.Prettify(Json.Serialize(Service.Instance.Graph));
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(Graph graph)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await Service.Instance.Restart(graph);

        return RedirectToPage("./Graph");
    }
}