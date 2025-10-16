// ToDoList.cs
// Single-file C# console application (target: .NET 6+)
// Features:
// - Add / List / Edit / Delete tasks
// - Mark complete / incomplete
// - Search and filter
// - Save/load to JSON file (todos.json)
// - Simple due date parsing and sorting
// How to run:
// 1) Install .NET 6+ SDK (https://dotnet.microsoft.com/)
// 2) Save this file as ToDoList.cs
// 3) Run: dotnet run (if in a project) or create a project and replace Program.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Todo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsDone { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class Storage
{
    private static readonly string FilePath = "todos.json";

    public static List<Todo> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new List<Todo>();
            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<Todo>();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<Todo>>(json, opts) ?? new List<Todo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load todos: {ex.Message}");
            return new List<Todo>();
        }
    }

    public static void Save(List<Todo> todos)
    {
        try
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(todos, opts);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save todos: {ex.Message}");
        }
    }
}

class Program
{
    static List<Todo> todos = new();

    static void Main()
    {
        Console.WriteLine("Welcome to your C# To-Do List! (console)");
        todos = Storage.Load();

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine()?.Trim();
            switch (choice)
            {
                case "1": AddTodo(); break;
                case "2": ListTodos(); break;
                case "3": EditTodo(); break;
                case "4": ToggleDone(); break;
                case "5": DeleteTodo(); break;
                case "6": SearchTodos(); break;
                case "7": ClearCompleted(); break;
                case "0": Exit(); return;
                default: Console.WriteLine("Invalid choice — try again."); break;
            }
        }
    }

    static void ShowMenu()
    {
        Console.WriteLine();
        Console.WriteLine("Menu:");
        Console.WriteLine("1) Add task");
        Console.WriteLine("2) List tasks");
        Console.WriteLine("3) Edit task");
        Console.WriteLine("4) Mark/Unmark done");
        Console.WriteLine("5) Delete task");
        Console.WriteLine("6) Search tasks");
        Console.WriteLine("7) Clear completed tasks");
        Console.WriteLine("0) Exit");
        Console.Write("Choose: ");
    }

    static void AddTodo()
    {
        Console.Write("Title: ");
        var title = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(title)) { Console.WriteLine("Title cannot be empty."); return; }

        Console.Write("Description (optional): ");
        var desc = Console.ReadLine();

        Console.Write("Due date (yyyy-MM-dd) optional: ");
        var dueInput = Console.ReadLine()?.Trim();
        DateTime? due = null;
        if (!string.IsNullOrWhiteSpace(dueInput) && DateTime.TryParse(dueInput, out var d)) due = d.Date;

        var todo = new Todo { Title = title!, Description = desc, DueDate = due };
        todos.Add(todo);
        Storage.Save(todos);
        Console.WriteLine("Task added.");
    }

    static void ListTodos()
    {
        if (!todos.Any()) { Console.WriteLine("No tasks yet."); return; }

        Console.WriteLine("Sort by: 1) Created 2) Due date 3) Status");
        Console.Write("Choice: ");
        var sort = Console.ReadLine()?.Trim();

        IEnumerable<Todo> list = todos;
        list = sort switch
        {
            "2" => todos.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ThenBy(t => t.CreatedAt),
            "3" => todos.OrderBy(t => t.IsDone).ThenBy(t => t.CreatedAt),
            _ => todos.OrderBy(t => t.CreatedAt)
        };

        Console.WriteLine();
        foreach (var t in list)
        {
            Console.WriteLine($"[{(t.IsDone ? 'X' : ' ')}] {t.Id} - {t.Title}");
            if (!string.IsNullOrWhiteSpace(t.Description)) Console.WriteLine($"    {t.Description}");
            Console.WriteLine($"    Due: {(t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd") : "—")} | Created: {t.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm}");
        }
    }

    static Todo? FindByIdInput()
    {
        Console.Write("Enter task ID: ");
        var id = Console.ReadLine()?.Trim();
        if (!Guid.TryParse(id, out var gid)) { Console.WriteLine("Invalid ID format."); return null; }
        var todo = todos.FirstOrDefault(t => t.Id == gid);
        if (todo == null) Console.WriteLine("Task not found.");
        return todo;
    }

    static void EditTodo()
    {
        var todo = FindByIdInput();
        if (todo == null) return;

        Console.Write($"New title (leave empty to keep: {todo.Title}): ");
        var title = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(title)) todo.Title = title.Trim();

        Console.Write($"New description (leave empty to keep): ");
        var desc = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(desc)) todo.Description = desc.Trim();

        Console.Write($"New due date yyyy-MM-dd (leave empty to keep '{(todo.DueDate?.ToString("yyyy-MM-dd") ?? "—")}'): ");
        var dueInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(dueInput))
        {
            if (DateTime.TryParse(dueInput.Trim(), out var d)) todo.DueDate = d.Date;
            else Console.WriteLine("Invalid date — keeping old value.");
        }

        Storage.Save(todos);
        Console.WriteLine("Task updated.");
    }

    static void ToggleDone()
    {
        var todo = FindByIdInput();
        if (todo == null) return;
        todo.IsDone = !todo.IsDone;
        Storage.Save(todos);
        Console.WriteLine(todo.IsDone ? "Marked as done." : "Marked as not done.");
    }

    static void DeleteTodo()
    {
        var todo = FindByIdInput();
        if (todo == null) return;
        Console.Write("Are you sure you want to delete this task? (y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();
        if (confirm == "y" || confirm == "yes")
        {
            todos.Remove(todo);
            Storage.Save(todos);
            Console.WriteLine("Deleted.");
        }
        else Console.WriteLine("Canceled.");
    }

    static void SearchTodos()
    {
        Console.Write("Search term (title/description): ");
        var q = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(q)) { Console.WriteLine("Empty query."); return; }
        var results = todos.Where(t => (t.Title ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) || (t.Description ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!results.Any()) { Console.WriteLine("No matches."); return; }
        foreach (var t in results)
        {
            Console.WriteLine($"[{(t.IsDone ? 'X' : ' ')}] {t.Id} - {t.Title} (Due: {(t.DueDate?.ToString("yyyy-MM-dd") ?? "—")})");
        }
    }

    static void ClearCompleted()
    {
        var completed = todos.Where(t => t.IsDone).ToList();
        if (!completed.Any()) { Console.WriteLine("No completed tasks to clear."); return; }
        Console.Write($"Delete {completed.Count} completed tasks? (y/n): ");
        var ans = Console.ReadLine()?.Trim().ToLower();
        if (ans == "y" || ans == "yes")
        {
            todos.RemoveAll(t => t.IsDone);
            Storage.Save(todos);
            Console.WriteLine("Cleared completed tasks.");
        }
        else Console.WriteLine("Canceled.");
    }

    static void Exit()
    {
        Storage.Save(todos);
        Console.WriteLine("Goodbye — your tasks are saved to todos.json");
    }
}
