using System;
using System.Collections.Generic;
using System.Linq;

/* DUNGEON CRAWLER CHARACTER MANAGEMENT - MARK KATIGBAK */

namespace Dungeon_Crawler_Character_Management
{
    // CREATE CLASS Character TO STORE ALL INFO ABOUT ONE CHARACTER
    public class Character
    {
        // BASIC DETAILS ABOUT THE CHARACTER
        private string _name;
        private string _class;
        // LIST OF SKILLS THE CHARACTER KNOWS
        private readonly List<Skill> _skills = new List<Skill>();

        public string Name
        {
            get => _name;
            private set => _name = value ?? throw new ArgumentNullException(nameof(value));
        }
        public string Class
        {
            get => _class;
            private set => _class = value ?? throw new ArgumentNullException(nameof(value));
        }

        // CHARACTER STATS
        public int Level { get; private set; } = 1;                 // EVERYONE STARTS AT LEVEL 1
        public int HitPoints { get; private set; }                  // CHARACTER LIFE
        public int AvailableAttributePoints { get; private set; }   // POINTS TO SPEND ON SKILLS
        public IReadOnlyList<Skill> Skills => _skills.AsReadOnly();

        public Character(string name, string characterClass, int attributePoints)
        {
            Name = name;
            Class = characterClass;
            AvailableAttributePoints = attributePoints;
            HitPoints = CalculateInitialHitPoints(attributePoints);
        }

        // CALCULATES HOW MUCH HEALTH A CHARACTER STARTS WITH
        private static int CalculateInitialHitPoints(int attributePoints) => 10 + (attributePoints / 2);

        // MAKES THE CHARACTER STRONGER WHEN THEY LEVEL UP
        public void LevelUp()
        {
            Level++;                        // INCREASE LEVEL BY 1
            HitPoints += 5;                 // GIVE MORE HEALTH
            AvailableAttributePoints += 10; // GIVE MORE POINTS TO SPEND ON SKILLS
        }

        // CHECKS IF THE CHARACTER CAN LEARN A NEW SKILL
        public bool TryAddSkill(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            // CAN'T LEARN A SKILL THEY ALREADY KNOW
            if (_skills.Any(s => s.Name == skill.Name))
                return false;
            // CAN'T LEARN IF THEY DON'T HAVE ENOUGH POINTS
            if (AvailableAttributePoints < skill.RequiredAttributePoints)
                return false;

            // LEARN THE SKILL AND PAY THE POINT COST
            _skills.Add(skill);
            AvailableAttributePoints -= skill.RequiredAttributePoints;
            
            return true;
        }

        // SHOWS ALL THE CHARACTER'S INFORMATION AS TEXT
        public override string ToString()
        {
            // START WITH BASIC INFO
            var result = $"Name: {Name}, Class: {Class}, Level: {Level}, HitPoints: {HitPoints}, Available Attribute Points: {AvailableAttributePoints}\n";
            result += "Skills:\n";

            // SHOW WHAT SKILLS THEY KNOW
            if (!Skills.Any())
            {
                result += "There are no skills assigned yet...!\n";
            }
            else
            {
                foreach (var skill in Skills)
                {
                    result += skill.ToString() + "\n";
                }
            }

            return result;
        }
    }

    // DESCRIBES WHAT A SKILL IS AND WHAT IT DOES
    public class Skill
    {
        // BASIC INFORMATION ABOUT THE SKILL
        private string _name;                   // WHAT THE SKILL IS CALLED
        private string _description;            // WHAT THE SKILL DOES
        private string _attribute;              // WHAT TYPE OF SKILL IT IS
        private int _requiredAttributePoints;   // HOW MANY POINTS IT COST TO LEARN

        public string Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(value));
        }
        public string Description
        {
            get => _description;
            set => _description = value ?? throw new ArgumentNullException(nameof(value));
        }
        public string Attribute
        {
            get => _attribute;
            set => _attribute = value ?? throw new ArgumentNullException(nameof(value));
        }
        public int RequiredAttributePoints
        {
            get => _requiredAttributePoints;
            set => _requiredAttributePoints = value >= 0 ? value : throw new ArgumentException("Required points cannot be negative");
        }

        public override string ToString() => $"{Name} - {Description} - {Attribute} - Point Requirement:{RequiredAttributePoints}";
    }

    // MANAGES ALL THE CHARACTERS AND AVAILABLE SKILLS IN THE GAME
    public class GameManager
    {
        // KEEP TRACK OF ALL CHARACTERS AND WHAT SKILLS EXIST IN THE GAME
        private readonly List<Character> _characters = new List<Character>();
        private readonly List<Skill> _availableSkills;

        public GameManager()
        {
            // LIST OF ALL THE SKILLS AVAILABLE TO LEARN
            _availableSkills = new List<Skill>
            {
                // EACH SKILL HAS A NAME, WHAT IT DOES, WHAT TYPE IT IS, AND HOW MUCH IT COSTS
                new Skill { Name = "Strike", Description = "A powerful strike.", Attribute = "Strength", RequiredAttributePoints = 10 },
                new Skill { Name = "Dodge", Description = "Avoid an attack.", Attribute = "Dexterity", RequiredAttributePoints = 15 },
                new Skill { Name = "Spellcast", Description = "Cast a spell.", Attribute = "Intelligence", RequiredAttributePoints = 20 }
            };
        }

        // ADD A NEW CHARACTER TO THE GAME
        public void CreateCharacter(string name, string characterClass, int attributePoints)
        {
            var character = new Character(name, characterClass, attributePoints);
            _characters.Add(character);
        }

        // TRY TO TEACH A CHARACTER A NEW SKILL
        public bool TryAssignSkill(string characterName, int skillIndex, out string message)
        {
            // FIND CHARACTER BY NAME
            var character = FindCharacter(characterName);
            
            // IF WE CAN'T FIND THEM, TELL THE PLAYER
            if (character == null)
            {
                message = "Character not found!";
                
                return false;
            }

            // MAKE SURE THEY PICKED A VALID SKILL NUMBER
            if (skillIndex < 1 || skillIndex > _availableSkills.Count)
            {
                message = "Invalid skill selection!";

                return false;
            }

            var selectedSkill = _availableSkills[skillIndex - 1];

            // CHECK IF THEY HAVE ENOUGH POINTS
            if (character.AvailableAttributePoints < selectedSkill.RequiredAttributePoints)
            {
                message = "Not enough attribute points are available!";
                return false;
            }

            // CHECK IF THEY ALREADY KNOW THIS SKILL
            if (character.Skills.Any(s => s.Name == selectedSkill.Name))
            {
                message = $"{character.Name} already has {selectedSkill.Name} skills!";
                return false;
            }

            // TRY TO TEACH THEM THE SKILL
            if (character.TryAddSkill(selectedSkill))
            {
                message = $"Skill: {selectedSkill.Name} added to {character.Name}";
                return true;
            }

            message = "Failed to add skill";
            return false;
        }

        // MAKE A CHARACTER STRONGER BY LEVELING THEM UP
        public bool TryLevelUpCharacter(string characterName, out string message)
        {
            // FIND CHARACTER BY NAME
            var character = FindCharacter(characterName);
            
            // IF WE CAN'T FIND THEM, TELL THE PLAYER
            if (character == null)
            {
                message = "Character not found!";
                
                return false;
            }

            // LEVEL THEM UP AND TELL THE PLAYER
            character.LevelUp();
            message = $"{character.Name} is now a Level:{character.Level} Character.";
            
            return true;
        }

        // GET A LIST OF ALL CHARACTERS IN THE GAME
        public IEnumerable<Character> GetAllCharacters() => _characters.AsReadOnly();
        // GET A LIST OF ALL SKILLS THAT EXIST IN THE GAME
        public IEnumerable<Skill> GetAvailableSkills() => _availableSkills.AsReadOnly();

        // HELPER METHOD TO FIND CHARACTER BY THEIR NAME
        private Character FindCharacter(string name) =>
            _characters.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // SHOWING THE MENUS AND GETTING INPUT FROM THE PLAYER
    public class GameUI
    {
        private readonly GameManager _gameManager;

        public GameUI(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        // RUNS THE MAIN GAME MENU IN A LOOP UNTIL THE PLAYER QUITS
        public void Start()
        {
            while (true)
            {
                DisplayMenu();
                var choice = Console.ReadLine();

                // DO DIFFERENT THINGS BASED ON WHAT NUMBER THE PLAYER PICKED
                switch (choice)
                {
                    case "1":
                        HandleCreateCharacter();    // MAKE A NEW CHARACTER
                        break;
                    case "2":
                        HandleAssignSkill();        // LEARN A NEW SKILL
                        break;
                    case "3":
                        HandleLevelUp();            // MAKE A CHARACTER LEVEL UP
                        break;
                    case "4":
                        DisplayCharacters();        // SHOW ALL CHARACTERS
                        break;
                    case "5":
                        return;                     // QUIT THE GAME
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        // SHOWS THE MAIN MENU OPTIONS TO THE PLAYER
        private void DisplayMenu()
        {
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1. Create a character");
            Console.WriteLine("2. Assign skills");
            Console.WriteLine("3. Level up a character");
            Console.WriteLine("4. Display all character sheets");
            Console.WriteLine("5. Exit");
            Console.Write("Enter your choice: ");
        }

        // HANDLES MAKING A NEW CHARACTER
        private void HandleCreateCharacter()
        {
            // ASK FOR CHARACTER'S DETAILS
            Console.Write("Enter name: ");
            string name = Console.ReadLine();
            Console.Write("Enter class: ");
            string characterClass = Console.ReadLine();
            Console.Write("Enter Total Attribute Points: ");
            if (int.TryParse(Console.ReadLine(), out int points))
            {
                _gameManager.CreateCharacter(name, characterClass, points);
            }
        }

        // HANDLES THE PROCESS OF LEARNING A NEW SKILL
        private void HandleAssignSkill()
        {
            // ASK WHICH CHARACTER SHOULD LEARN THE SKILL
            Console.Write("Enter character name: ");
            string name = Console.ReadLine();

            // SHOW HOW MANY POINTS THE CHARACTER HAS TO SPEND
            var character = _gameManager.GetAllCharacters().FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (character != null)
            {
                Console.WriteLine($"\nTotal Attribute Points Available for this character: {character.AvailableAttributePoints}");
            }

            // SHOW WHAT SKILLS THEY CAN LEARN
            var skills = _gameManager.GetAvailableSkills().ToList();
            Console.WriteLine("Available skills:");
            for (int i = 0; i < skills.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {skills[i]}");
            }

            // KEEP ASKING UNTIL THEY PICK A VALID SKILL NUMBER
            int skillChoice;
            bool isValidChoice;
            Console.Write("Select a skill to assign: ");
            do
            {
                string input = Console.ReadLine();
                isValidChoice = int.TryParse(input, out skillChoice) && skillChoice >= 1 && skillChoice <= 3;

                if (!isValidChoice)
                {
                    Console.WriteLine("Invalid selection. Please enter a number in range (1..3):");
                }
            } while (!isValidChoice);
                
            if (_gameManager.TryAssignSkill(name, skillChoice, out string message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        // HANDLES MAKING A CHARACTER LEVEL UP
        private void HandleLevelUp()
        {
            // ASK WHICH CHARACTER TO LEVEL UP
            Console.Write("Enter character name: ");
            string name = Console.ReadLine();

            // TRY TO LEVEL UP AND SHOW IF IT WORKED
            if (_gameManager.TryLevelUpCharacter(name, out string message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        // SHOWS ALL CHARACTERS THAT HAVE BEEN CREATED
        private void DisplayCharacters()
        {
            Console.WriteLine("All Characters in the character sheet.......................");
            foreach (var character in _gameManager.GetAllCharacters())
            {
                Console.WriteLine(character);
            }
            Console.WriteLine("End.........................................................\n");
        }
    }

    // WHERE THE GANE STARTS
    internal class Program
    {
        static void Main(string[] args)
        {
            // CREATE THE GAME AND START IT
            var gameManager = new GameManager();
            var gameUI = new GameUI(gameManager);
            gameUI.Start();
        }
    }
}
