using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;

public static class Name_Generator
{
    //Random Chances
    // If random == 0 then yes. rand(0,Random Chances);
    const int chanceOfPossesive = 3;
    const int chanceOfUsingLastName = 2;
    const int chanceOfStoppingAtNoun = 4;

    public static string GenerateFirstName(int seed, int age)
    {
        Random rnd = new Random(seed);
        if (age < 10)
        {
            return firstNames1970s[rnd.Next() % firstNames1970s.Count];
        }
        else if (age < 20)
        {
            return firstNames1960s[rnd.Next() % firstNames1960s.Count];
        }
        else if (age < 30)
        {
            return firstNames1950s[rnd.Next() % firstNames1950s.Count];
        }
        else if (age < 40)
        {
            return firstNames1940s[rnd.Next() % firstNames1940s.Count];
        }
        else if (age < 50)
        {
            return firstNames1930s[rnd.Next() % firstNames1930s.Count];
        }
        else if (age < 60)
        {
            return firstNames1920s[rnd.Next() % firstNames1920s.Count];
        }
        else if (age < 70)
        {
            return firstNames1910s[rnd.Next() % firstNames1910s.Count];
        }
        else
        {
            return firstNames1900s[rnd.Next() % firstNames1900s.Count];
        }
    }

    public static string GenerateLastName(int seed)
    {
        Random rnd = new Random(seed);
        return lastNames[rnd.Next() % lastNames.Count];
    }

    public static string GenerateLocationName(int seed, string firstName, string lastName)
    {
        Random rnd = new Random(seed);
        List<PartsofSpeech> nameSections = new List<PartsofSpeech>
        {
            (PartsofSpeech)(rnd.Next() % (Enum.GetValues(typeof(PartsofSpeech)).Length-1)),
        };

        int doubleCheckIndex = 0;
        bool hasUsedNoun = false;
        bool adjectiveUsedFirst = false;
        int adjectiveCount = 0;
        if (nameSections[0] == PartsofSpeech.adjective)
        {
            adjectiveUsedFirst = true;
        }
        bool breakOut = false;
        while (nameSections[nameSections.Count-1] != PartsofSpeech.place || breakOut)
        {
            doubleCheckIndex++;
            if(doubleCheckIndex > 10)
            {
                break;
            }
            switch (nameSections[nameSections.Count - 1])
            {
                case PartsofSpeech.firstName:
                    if((rnd.Next() % chanceOfUsingLastName) == 0 && adjectiveUsedFirst == false)
                    {
                        nameSections.Add(PartsofSpeech.lastName);
                    }
                    else
                    {
                        nameSections.Add(GetRandomSection(new List<PartsofSpeech>() { PartsofSpeech.noun, PartsofSpeech.adjective, PartsofSpeech.place }, rnd));
                    }
                    break;
                case PartsofSpeech.lastName:
                    nameSections.Add(GetRandomSection(new List<PartsofSpeech>() { PartsofSpeech.noun, PartsofSpeech.adjective, PartsofSpeech.place }, rnd));
                    break;
                case PartsofSpeech.noun:
                    hasUsedNoun = true;
                    if(rnd.Next() % chanceOfStoppingAtNoun == 0)
                    {
                        breakOut = true;
                        break;
                    }
                    nameSections.Add(GetRandomSection(new List<PartsofSpeech>() {PartsofSpeech.adjective, PartsofSpeech.place }, rnd));
                    break;
                case PartsofSpeech.adjective:
                    adjectiveCount++;
                    if(adjectiveUsedFirst && nameSections.Count == 1)
                    {
                        nameSections.Add(GetRandomSection(new List<PartsofSpeech>() { PartsofSpeech.firstName, PartsofSpeech.adjective, PartsofSpeech.noun }, rnd));
                    }
                    else
                    {
                        if (adjectiveCount < 1)
                        {
                            if (hasUsedNoun)
                            {
                                nameSections.Add(GetRandomSection(new List<PartsofSpeech>() {PartsofSpeech.adjective,PartsofSpeech.place }, rnd));
                            }
                            else
                            {
                                nameSections.Add(GetRandomSection(new List<PartsofSpeech>() { PartsofSpeech.adjective, PartsofSpeech.place,PartsofSpeech.noun }, rnd));
                            }
                        }
                        else
                        {
                            if (hasUsedNoun)
                            {
                                nameSections.Add(PartsofSpeech.place);
                            }
                            else
                            {
                                nameSections.Add(GetRandomSection(new List<PartsofSpeech>() { PartsofSpeech.noun, PartsofSpeech.place }, rnd));
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        string fullName = "";
        for (int i = 0; i < nameSections.Count; i++)
        {
            if (i > 0)
            {
                fullName += " ";
            }
            switch (nameSections[i])
            {
                case PartsofSpeech.firstName:
                    fullName += firstName;
                    if (nameSections[i + 1] != PartsofSpeech.lastName && (rnd.Next() % chanceOfPossesive) == 0)
                    {
                        if (firstName[firstName.Length - 1] == 's')
                        {
                            fullName += "'";
                        }
                        else
                        {
                            fullName += "'s";
                        }
                    }
                    break;
                case PartsofSpeech.lastName:
                    fullName += lastName;
                    if ((rnd.Next() % chanceOfPossesive) == 0)
                    {
                        if (firstName[firstName.Length - 1] == 's')
                        {
                            fullName += "'";
                        }
                        else
                        {
                            fullName += "'s";
                        }
                    }
                    break;
                case PartsofSpeech.noun:
                    fullName += placeNouns[rnd.Next() % placeNouns.Count];
                    break;
                case PartsofSpeech.adjective:
                    fullName += placeAdjective[rnd.Next() % placeAdjective.Count];
                    break;
                case PartsofSpeech.place:
                    fullName += placePlaces[rnd.Next() % placePlaces.Count];
                    break;
                default:
                    break;
            }
        }
        return fullName;
    }

    static PartsofSpeech GetRandomSection(List<PartsofSpeech> parts, Random rnd)
    {
        return parts[rnd.Next() % parts.Count];
    }

    public enum PartsofSpeech
    {
        firstName,
        lastName,
        noun,
        adjective,
        place,
    }

    static readonly ReadOnlyCollection<string> firstNames1900s = new ReadOnlyCollection<string>(
new string[] {
        "John","Mary","William","Helen","James","Margaret","George","Anna","Charles","Ruth","Robert","Elizabeth","Joseph","Dorothy","Frank","Marie","Edward","Florence","Thomas","Mildred","Henry","Alice","Walter","Ethel","Harry","Lillian","Willie","Gladys","Arthur","Edna","Albert","Frances","Clarence","Rose","Fred","Annie","Harold","Grace","Paul","Bertha","Raymond","Emma","Richard","Bessie","Roy","Clara","Joe","Hazel","Louis","Irene","Carl","Gertrude","Ralph","Louise","Earl","Catherine","Jack","Martha","Ernest","Mabel","David","Pearl","Samuel","Edith","Howard","Esther","Charlie","Minnie","Francis","Myrtle","Herbert","Ida","Lawrence","Josephine","Theodore","Evelyn","Alfred","Elsie","Andrew","Eva","Elmer","Thelma","Sam","Ruby","Eugene","Agnes","Leo","Sarah","Michael","Viola","Lee","Nellie","Herman","Beatrice","Anthony","Julia","Daniel","Laura","Leonard","Lillie","Floyd","Lucille","Donald","Ella","Kenneth","Virginia","Jesse","Mattie","Russell","Pauline","Clyde","Carrie","Oscar","Alma","Peter","Jessie","Lester","Mae","Leroy","Lena","Ray","Willie","Stanley","Katherine","Clifford","Blanche","Lewis","Hattie","Benjamin","Marion","Edwin","Lucy","Frederick","Stella","Chester","Mamie","Claude","Vera","Eddie","Cora","Cecil","Fannie","Lloyd","Eleanor","Jessie","Bernice","Martin","Jennie","Bernard","Ann","Tom","Leona","Will","Beulah","Norman","Lula","Edgar","Rosa","Harvey","Ada","Ben","Ellen","Homer","Kathryn","Luther","Maggie","Leon","Doris","Melvin","Dora","Philip","Betty","Johnnie","Marguerite","Jim","Violet","Milton","Lois","Everett","Daisy","Allen","Anne","Leslie","Sadie","Alvin","Susie","Victor","Nora","Marvin","Georgia","Stephen","Maude","Alexander","Marjorie","Jacob","Opal","Hugh","Hilda","Patrick","Velma","Virgil","Emily","Horace","Theresa","Glenn","Charlotte","Oliver","Inez","Morris","Olive","Vernon","Flora","Archie","Della","Julius","Lola","Gerald","Jean","Sidney","Effie","Maurice","Nancy","Marion","Nettie","Otis","Sylvia","Vincent","May","Guy","Lottie","Earnest","Alberta","Wilbur","Eunice","Gilbert","Sallie","Willard","Katie","Ed","Genevieve","Roosevelt","Estelle","Hubert","Lydia","Manuel","Loretta","Warren","Mable","Otto","Goldie","Alex","Eula","Ira","Rosie","Wesley","Lizzie","Curtis","Vivian","Wallace","Verna","Lonnie","Ollie","Gordon","Harriet","Isaac","Lucile","Jerry","Addie","Charley","Marian","Jose","Henrietta","Nathan","Jane","Max","Lela","Mack","Essie","Rufus","Caroline","Arnold","Ora","Irving","Iva","Percy","Sara","Bill","Maria","Dan","Madeline","Willis","Rebecca","Bennie","Wilma","Jimmie","Etta","Orville","Barbara","Sylvester","Rachel","Rudolph","Kathleen","Horace","Irma","Glenn","Christine","Nicholas","Geneva","Emil","Sophie","Roland","Juanita","Steve","Nina","Calvin","Naomi","Mike","Victoria","Johnie","Amelia","Bert","Erma","August","Mollie","Clifton","Susan","Franklin","Flossie","Matthew","Ola","Emmett","Nannie","Phillip","Norma","Wayne","Sally","Edmund","Olga","Abraham","Alta","Nathaniel","Estella","Marshall","Celia","Dave","Freda","Elbert","Isabel","Clinton","Amanda","Felix","Frieda","Alton","Luella","Ellis","Matilda","Nelson","Janie","Amos","Fern","Clayton","Cecelia","Aaron","Audrey","Perry","Winifred","Adam","Elva","Tony","Ina","Irvin","Adeline","Jake","Leola","Dennis","Hannah","Jerome","Geraldine","Mark","Amy","Cornelius","Allie","Ollie","Miriam","Douglas","Isabelle","Pete","Bonnie","Ted","Virgie","Adolph","Sophia","Roger","Cleo","Jay","Jeanette","Roscoe","Nell","Juan","Eliza"
}
);

    static readonly ReadOnlyCollection<string> firstNames1910s = new ReadOnlyCollection<string>(
new string[] {
        "John", "Mary", "William", "Helen", "James", "Dorothy", "Robert", "Margaret", "Joseph", "Ruth", "George", "Mildred", "Charles", "Anna", "Edward", "Elizabeth", "Frank", "Frances", "Thomas", "Virginia", "Walter", "Marie", "Harold", "Evelyn", "Henry", "Alice", "Paul", "Florence", "Richard", "Lillian", "Raymond", "Rose", "Albert", "Irene", "Arthur", "Louise", "Harry", "Edna", "Donald", "Catherine", "Ralph", "Gladys", "Louis", "Ethel", "Jack", "Josephine", "Clarence", "Ruby", "Carl", "Martha", "Willie", "Grace", "Howard", "Hazel", "Fred", "Thelma", "David", "Lucille", "Kenneth", "Edith", "Francis", "Eleanor", "Roy", "Doris", "Earl", "Annie", "Joe", "Pauline", "Ernest", "Gertrude", "Lawrence", "Esther", "Stanley", "Betty", "Anthony", "Beatrice", "Eugene", "Marjorie", "Samuel", "Clara", "Herbert", "Emma", "Alfred", "Bernice", "Leonard", "Bertha", "Michael", "Ann", "Elmer", "Jean", "Andrew", "Elsie", "Leo", "Julia", "Bernard", "Agnes", "Norman", "Lois", "Peter", "Sarah", "Russell", "Marion", "Daniel", "Katherine", "Edwin", "Eva", "Frederick", "Ida", "Chester", "Bessie", "Herman", "Pearl", "Lloyd", "Anne", "Melvin", "Viola", "Lester", "Myrtle", "Floyd", "Nellie", "Leroy", "Mabel", "Theodore", "Laura", "Clifford", "Kathryn", "Clyde", "Stella", "Charlie", "Vera", "Sam", "Willie", "Woodrow", "Jessie", "Vincent", "Jane", "Philip", "Alma", "Marvin", "Minnie", "Ray", "Sylvia", "Lewis", "Ella", "Milton", "Lillie", "Benjamin", "Rita", "Victor", "Leona", "Vernon", "Barbara", "Gerald", "Vivian", "Jesse", "Lena", "Martin", "Violet", "Cecil", "Lucy", "Alvin", "Jennie", "Lee", "Genevieve", "Willard", "Marguerite", "Leon", "Charlotte", "Oscar", "Mattie", "Glenn", "Marian", "Edgar", "Blanche", "Gordon", "Mae", "Stephen", "Ellen", "Harvey", "Wilma", "Sidney", "Juanita", "Claude", "Opal", "Everett", "June", "Arnold", "Geraldine", "Morris", "Beulah", "Wilbur", "Velma", "Warren", "Theresa", "Wayne", "Carrie", "Allen", "Phyllis", "Homer", "Maxine", "Maurice", "Nancy", "Alexander", "Emily", "Max", "Georgia", "Virgil", "Fannie", "Gilbert", "Kathleen", "Irving", "Hattie", "Leslie", "Inez", "Eddie", "Sophie", "Johnnie", "Rosa", "Nicholas", "Lorraine", "Hugh", "Hilda", "Julius", "Harriet", "Jessie", "Norma", "Marion", "Eunice", "Luther", "Sara", "Steve", "Cora", "Hubert", "Ada", "Ben", "Geneva", "Curtis", "Alberta", "Roland", "Loretta", "Jacob", "Mamie", "Wallace", "Christine", "Oliver", "Dora", "Glen", "Lula", "Horace", "Estelle", "Roger", "Verna", "Manuel", "Audrey", "Dale", "Madeline", "Franklin", "Shirley", "Mike", "Eileen", "Orville", "Daisy", "Alex", "Sadie", "Tom", "Olive", "Wesley", "Naomi", "Tony", "Lola", "Edmund", "Flora", "Jerome", "Lucile", "Willis", "Olga", "Nathan", "Mable", "Otis", "Muriel", "Archie", "Susie", "Douglas", "Maggie", "Rudolph", "Maria", "Earnest", "Jeanette", "Wilson", "Nora", "Emil", "Miriam", "Guy", "Erma", "Bill", "Dolores", "Jerry", "Victoria", "Matthew", "Wanda", "Patrick", "Janet", "Ira", "Lottie", "Clifton", "Caroline", "Angelo", "Rachel", "Abraham", "Irma", "Salvatore", "Henrietta", "Jose", "Roberta", "Jim", "Winifred", "Jimmie", "Eula", "Calvin", "Patricia", "Don", "Rosie", "Lyle", "Anita", "Bennie", "Rebecca", "Clayton", "Della", "Marshall", "Nettie", "Bruce", "Sally", "Otto", "Fern", "Sylvester", "Lydia", "Clinton", "Adeline", "Ronald", "Carolyn", "Wilbert", "Amelia", "Irvin", "Nina", "Delbert", "Jeanne", "Phillip", "Elaine", "Ervin", "Goldie", "Elbert", "Katie", "Wilfred", "Bonnie", "Isaac", "Antoinette", "Ivan", "May", "Felix", "Marcella", "Rufus", "Lorene", "August", "Essie", "Forrest", "Arlene", "Nathaniel", "Ollie", "Dan", "Dorothea", "Nelson", "Jeannette", "Karl", "Veronica", "Nick", "Effie", "Julian", "Isabel", "Merle", "Freda", "Aaron", "Regina", "Lonnie", "Sallie", "Adolph", "Cleo", "Adam", "Ora", "Jay", "Isabelle", "Ellis", "Rosemary", "Alton", "Addie", "Leland", "Eloise", "Pete", "Joan", "Bert", "Lela"
}
);

    static readonly ReadOnlyCollection<string> firstNames1920s = new ReadOnlyCollection<string>(
  new string[] {
        "Robert", "Mary", "John", "Dorothy", "James", "Helen", "William", "Betty", "Charles", "Margaret", "George", "Ruth", "Joseph", "Virginia", "Richard", "Doris", "Edward", "Mildred", "Donald", "Frances", "Thomas", "Elizabeth", "Frank", "Evelyn", "Harold", "Anna", "Paul", "Marie", "Raymond", "Alice", "Walter", "Jean", "Jack", "Shirley", "Henry", "Barbara", "Kenneth", "Irene", "Arthur", "Marjorie", "Albert", "Florence", "David", "Lois", "Harry", "Martha", "Eugene", "Rose", "Ralph", "Lillian", "Howard", "Louise", "Carl", "Catherine", "Willie", "Ruby", "Louis", "Eleanor", "Clarence", "Patricia", "Earl", "Gladys", "Roy", "Annie", "Fred", "Josephine", "Joe", "Thelma", "Francis", "Edna", "Lawrence", "Norma", "Herbert", "Pauline", "Leonard", "Lucille", "Ernest", "Edith", "Alfred", "Gloria", "Anthony", "Ethel", "Stanley", "Phyllis", "Norman", "Grace", "Gerald", "Hazel", "Daniel", "June", "Samuel", "Bernice", "Bernard", "Marion", "Billy", "Dolores", "Melvin", "Rita", "Marvin", "Lorraine", "Warren", "Ann", "Michael", "Esther", "Leroy", "Beatrice", "Russell", "Juanita", "Leo", "Clara", "Andrew", "Jane", "Edwin", "Geraldine", "Elmer", "Sarah", "Peter", "Emma", "Floyd", "Joan", "Lloyd", "Joyce", "Ray", "Nancy", "Frederick", "Katherine", "Theodore", "Gertrude", "Clifford", "Elsie", "Vernon", "Julia", "Herman", "Agnes", "Clyde", "Wilma", "Chester", "Marian", "Philip", "Bertha", "Alvin", "Eva", "Lester", "Willie", "Wayne", "Audrey", "Vincent", "Theresa", "Gordon", "Vivian", "Leon", "Wanda", "Lewis", "Laura", "Charlie", "Charlotte", "Glenn", "Ida", "Calvin", "Elaine", "Martin", "Anne", "Milton", "Marilyn", "Lee", "Kathryn", "Jesse", "Maxine", "Dale", "Kathleen", "Cecil", "Viola", "Bill", "Pearl", "Harvey", "Vera", "Roger", "Bessie", "Victor", "Myrtle", "Benjamin", "Alma", "Wallace", "Beverly", "Ronald", "Violet", "Sam", "Nellie", "Allen", "Ella", "Arnold", "Lillie", "Willard", "Jessie", "Gilbert", "Jeanne", "Edgar", "Ellen", "Gene", "Lucy", "Jerry", "Minnie", "Douglas", "Sylvia", "Johnnie", "Donna", "Claude", "Leona", "Don", "Rosemary", "Eddie", "Stella", "Roland", "Mattie", "Everett", "Margie", "Curtis", "Mabel", "Virgil", "Geneva", "Wilbur", "Georgia", "Manuel", "Bonnie", "Stephen", "Carol", "Jerome", "Velma", "Homer", "Lena", "Leslie", "Carolyn", "Glen", "Mae", "Jessie", "Jennie", "Hubert", "Maria", "Jose", "Christine", "Jimmie", "Arlene", "Sidney", "Peggy", "Morris", "Marguerite", "Hugh", "Opal", "Max", "Sara", "Bobby", "Loretta", "Bob", "Harriet", "Nicholas", "Rosa", "Luther", "Muriel", "Bruce", "Eunice", "Junior", "Jeanette", "Wesley", "Blanche", "Rudolph", "Carrie", "Alexander", "Emily", "Franklin", "Beulah", "Tom", "Billie", "Irving", "Dora", "Horace", "Roberta", "Willis", "Hilda", "Patrick", "Naomi", "Steve", "Anita", "Johnny", "Jacqueline", "Dean", "Alberta", "Julius", "Inez", "Keith", "Delores", "Oliver", "Fannie", "Earnest", "Hattie", "Ben", "Lula", "Jim", "Verna", "Tony", "Cora", "Edmund", "Constance", "Lyle", "Madeline", "Guy", "Miriam", "Salvatore", "Ada", "Orville", "Claire", "Delbert", "Mamie", "Billie", "Lola", "Phillip", "Rosie", "Clayton", "Erma", "Otis", "Rachel", "Archie", "Mable", "Alex", "Flora", "Angelo", "Daisy", "Mike", "Sally", "Jacob", "Marcella", "Clifton", "Bette", "Bennie", "Olga", "Matthew", "Caroline", "Duane", "Laverne", "Clinton", "Sophie", "Dennis", "Nora", "Wilbert", "Rebecca", "Dan", "Estelle", "Jay", "Irma", "Marshall", "Susie", "Leland", "Eula", "Merle", "Winifred", "Ira", "Eloise", "Nathaniel", "Janice", "Ivan", "Maggie", "Ervin", "Antoinette", "Jimmy", "Nina", "Irvin", "Rosalie", "Alton", "Imogene", "Lowell", "Lorene", "Dewey", "Olive", "Larry", "Sadie", "Emil", "Regina", "Antonio", "Victoria", "Wilfred", "Henrietta", "Elbert", "Della", "Juan", "Bettie", "Alan", "Lila", "Allan", "Fern", "Lonnie",
  }
);

    static readonly ReadOnlyCollection<string> firstNames1930s = new ReadOnlyCollection<string>(
new string[] {
        "Robert", "Mary", "James", "Betty", "John", "Barbara", "William", "Shirley", "Richard", "Patricia", "Charles", "Dorothy", "Donald", "Joan", "George", "Margaret", "Thomas", "Nancy", "Joseph", "Helen", "David", "Carol", "Edward", "Joyce", "Ronald", "Doris", "Paul", "Ruth", "Kenneth", "Virginia", "Frank", "Marilyn", "Raymond", "Elizabeth", "Jack", "Jean", "Harold", "Frances", "Billy", "Beverly", "Gerald", "Lois", "Walter", "Alice", "Jerry", "Donna", "Joe", "Martha", "Eugene", "Dolores", "Henry", "Janet", "Bobby", "Phyllis", "Arthur", "Norma", "Carl", "Carolyn", "Larry", "Evelyn", "Ralph", "Gloria", "Albert", "Anna", "Willie", "Marie", "Fred", "Ann", "Michael", "Mildred", "Lawrence", "Rose", "Harry", "Peggy", "Howard", "Geraldine", "Roy", "Catherine", "Norman", "Judith", "Roger", "Louise", "Daniel", "Janice", "Louis", "Marjorie", "Earl", "Annie", "Gary", "Ruby", "Clarence", "Eleanor", "Anthony", "Jane", "Francis", "Sandra", "Wayne", "Irene", "Marvin", "Wanda", "Ernest", "Elaine", "Leonard", "June", "Herbert", "Joanne", "Melvin", "Rita", "Stanley", "Florence", "Leroy", "Delores", "Don", "Lillian", "Peter", "Marlene", "Jimmy", "Edna", "Alfred", "Sarah", "Dale", "Patsy", "Bill", "Lorraine", "Samuel", "Thelma", "Bernard", "Josephine", "Ray", "Juanita", "Gene", "Bonnie", "Philip", "Arlene", "Russell", "Gladys", "Frederick", "Sally", "Franklin", "Charlotte", "Dennis", "Kathleen", "Jimmie", "Audrey", "Gordon", "Pauline", "Andrew", "Wilma", "Theodore", "Sylvia", "Floyd", "Theresa", "Johnny", "Jacqueline", "Allen", "Clara", "Glenn", "Ethel", "Bruce", "Loretta", "Edwin", "Grace", "Lee", "Sharon", "Lloyd", "Edith", "Bob", "Lucille", "Clifford", "Emma", "Leon", "Bernice", "Leo", "Marion", "Clyde", "Linda", "Eddie", "Jo", "Vernon", "Anne", "Martin", "Hazel", "Alvin", "Roberta", "Jim", "Carole", "Herman", "Darlene", "Lewis", "Esther", "Harvey", "Katherine", "Tommy", "Ellen", "Vincent", "Laura", "Charlie", "Julia", "Warren", "Rosemary", "Jerome", "Jeanette", "Jesse", "Marian", "Patrick", "Willie", "Stephen", "Beatrice", "Curtis", "Margie", "Arnold", "Billie", "Gilbert", "Vivian", "Elmer", "Eva", "Lester", "Kathryn", "Duane", "Elsie", "Phillip", "Judy", "Cecil", "Eileen", "Tom", "Anita", "Alan", "Diane", "Milton", "Bertha", "Jackie", "Susan", "Victor", "Maria", "Johnnie", "Maxine", "Roland", "Ida", "Benjamin", "Yvonne", "Glen", "Ella", "Chester", "Lillie", "Calvin", "Constance", "Keith", "Sue", "Dean", "Bobbie", "Sam", "Georgia", "Wallace", "Jeanne", "Claude", "Christine", "Maurice", "Sara", "Willard", "Alma", "Manuel", "Bessie", "Jose", "Agnes", "Leslie", "Vera", "Edgar", "Nellie", "Marion", "Kay", "Max", "Jessie", "Hugh", "Karen", "Oscar", "Lucy", "Virgil", "Mattie", "Allan", "Gertrude", "Jessie", "Rosa", "Darrell", "Minnie", "Terry", "Gail", "Everett", "Connie", "Wesley", "Geneva", "Ted", "Viola", "Freddie", "Velma", "Jay", "Marcia", "Dick", "Leona", "Hubert", "Myrtle", "Nicholas", "Violet", "Rosalie", "Rodney", "Harriet", "Lowell", "Annette", "Dan", "Naomi", "Neil", "Charlene", "Sidney", "Pearl", "Homer", "Joy", "Delbert", "Mae", "Tony", "Suzanne", "Morris", "Claire", "Lyle", "Rebecca", "Wilbur", "Faye", "Luther", "Carrie", "Earnest", "Pat", "Ronnie", "Dora", "Bennie", "Rachel", "Joel", "Rosie", "Ben", "Emily", "Steve", "Eunice", "Rudolph", "Maureen", "Willis", "Alberta", "Horace", "Cora", "Lonnie", "Stella", "Mike", "Lula", "Junior", "Sheila", "Carroll", "Caroline", "Karl", "Glenda", "Guy", "Verna", "Roosevelt", "Lena", "Otis", "Lola", "Mark", "Myrna", "Nathaniel", "Hattie", "Danny", "Rosemarie", "Alton", "Ramona", "Marshall", "Gwendolyn", "Clayton", "Jeannette", "Alexander", "Erma", "Benny", "Genevieve", "Clifton", "Cynthia", "Archie", "Nina", "Oliver", "Patty", "Clinton", "Fannie", "Barry", "Diana", "Juan", "Jennie", "Salvatore", "Hilda", "Nelson", "Marguerite", "Jon", "Johnnie", "Alex"
}
);

    static readonly ReadOnlyCollection<string> firstNames1940s = new ReadOnlyCollection<string>(
new string[] {
        "James", "Mary", "Robert", "Linda", "John", "Barbara", "William", "Patricia", "Richard", "Carol", "David", "Sandra", "Charles", "Nancy", "Thomas", "Sharon", "Michael", "Judith", "Ronald", "Susan", "Larry", "Betty", "Donald", "Carolyn", "Joseph", "Margaret", "Gary", "Shirley", "George", "Judy", "Kenneth", "Karen", "Paul", "Donna", "Edward", "Kathleen", "Jerry", "Joyce", "Dennis", "Dorothy", "Frank", "Janet", "Daniel", "Diane", "Raymond", "Janice", "Roger", "Joan", "Stephen", "Elizabeth", "Gerald", "Brenda", "Walter", "Gloria", "Harold", "Virginia", "Steven", "Marilyn", "Douglas", "Martha", "Lawrence", "Beverly", "Terry", "Helen", "Wayne", "Bonnie", "Arthur", "Ruth", "Jack", "Frances", "Carl", "Jean", "Henry", "Ann", "Willie", "Phyllis", "Bruce", "Pamela", "Joe", "Jane", "Peter", "Alice", "Billy", "Peggy", "Roy", "Cheryl", "Ralph", "Doris", "Anthony", "Catherine", "Jimmy", "Elaine", "Albert", "Cynthia", "Bobby", "Marie", "Eugene", "Lois", "Johnny", "Connie", "Fred", "Christine", "Harry", "Diana", "Howard", "Gail", "Mark", "Joanne", "Alan", "Rose", "Louis", "Wanda", "Philip", "Carole", "Patrick", "Rita", "Dale", "Charlotte", "Danny", "Jo", "Stanley", "Evelyn", "Leonard", "Geraldine", "Timothy", "Jacqueline", "Gregory", "Ellen", "Samuel", "Sally", "Ronnie", "Rebecca", "Norman", "Kathryn", "Ernest", "Deborah", "Russell", "Norma", "Francis", "Suzanne", "Melvin", "Anna", "Earl", "Sue", "Frederick", "Darlene", "Allen", "Patsy", "Bill", "Joann", "Tommy", "Sarah", "Phillip", "Katherine", "Marvin", "Paula", "Steve", "Annie", "Don", "Louise", "Clarence", "Roberta", "Barry", "Sylvia", "Glenn", "Anne", "Jim", "Theresa", "Eddie", "Sheila", "Mike", "Maria", "Andrew", "Laura", "Jeffrey", "Kathy", "Leroy", "Eileen", "Alfred", "Marcia", "Rosemary", "Glenda", "Tom", "Dolores", "Ray", "Mildred", "Herbert", "Lorraine", "Gene", "Marjorie", "Bernard", "Sherry", "Theodore", "Kay", "Curtis", "Anita", "Keith", "Dianne", "Clifford", "Ruby", "Rodney", "Irene", "Gordon", "Juanita", "Jimmie", "Maureen", "Jesse", "Loretta", "Vincent", "Jeanne", "Warren", "Constance", "Lloyd", "Lynn", "Leon", "Marlene", "Jerome", "Arlene", "Edwin", "Delores", "Brian", "Lynda", "Victor", "Julia", "Bob", "Marsha", "Floyd", "Charlene", "Lewis", "June", "Harvey", "Jeanette", "Alvin", "Edna", "Clyde", "Josephine", "Craig", "Eleanor", "Vernon", "Yvonne", "Leslie", "Vicki", "Franklin", "Vivian", "Calvin", "Emma", "Jon", "Georgia", "Jay", "Lillian", "Charlie", "Edith", "Darrell", "Pauline", "Jackie", "Wilma", "Dan", "Victoria", "Allan", "Ethel", "Randall", "Lucille", "Joel", "Florence", "Gilbert", "Sara", "Benjamin", "Margie", "Lester", "Thelma", "Duane", "Clara", "Leo", "Audrey", "Tony", "Grace", "Herman", "Teresa", "Jose", "Annette", "Glen", "Claudia", "Johnnie", "Gladys", "Dean", "Julie", "Arnold", "Marion", "Lonnie", "Gwendolyn", "Christopher", "Priscilla", "Nicholas", "Willie", "Freddie", "Esther", "Chester", "Eva", "Eric", "Andrea", "Milton", "Maryann", "Cecil", "Bernice", "Lynn", "Rosa", "Manuel", "Hazel", "Randy", "Billie", "Roland", "Bertha", "Ted", "Gayle", "Dwight", "Bobbie", "Claude", "Pat", "Wesley", "Maxine", "Neil", "Marian", "Sam", "Ella", "Scott", "Paulette", "Dave", "Joy", "Wallace", "Beatrice", "Kevin", "Cathy", "Hugh", "Lillie", "Donnie", "Janis", "Elmer", "Leslie", "Micheal", "Ida", "Willard", "Lynne", "Juan", "Deanna", "Maurice", "Faye", "Jessie", "Terry", "Garry", "Valerie", "Marshall", "Emily", "Oscar", "Rachel", "Edgar", "Lucy", "Karl", "Janie", "Marion", "Penny", "Sidney", "Jackie", "Harriet", "Nathaniel", "Regina", "Alexander", "Rosalie", "Sammy", "Alma", "Everett", "Angela", "Benny", "Vera", "Guy", "Jessie", "Virgil", "Marianne", "Morris", "Stephanie", "Matthew", "Jill", "Earnest", "Mattie", "Lyle", "Minnie", "Max", "Caroline", "Bennie", "Michele", "Wendell", "Veronica", "Kent", "Patty", "Jonathan", "Rosie", "Fredrick", "Stella"
}
);

    static readonly ReadOnlyCollection<string> firstNames1950s = new ReadOnlyCollection<string>(
new string[] {
        "James", "Mary", "Michael", "Linda", "Robert", "Patricia", "John", "Susan", "David", "Deborah", "William", "Barbara", "Richard", "Debra", "Thomas", "Karen", "Mark", "Nancy", "Charles", "Donna", "Steven", "Cynthia", "Gary", "Sandra", "Joseph", "Pamela", "Donald", "Sharon", "Ronald", "Kathleen", "Kenneth", "Carol", "Paul", "Diane", "Larry", "Brenda", "Daniel", "Cheryl", "Stephen", "Janet", "Dennis", "Elizabeth", "Timothy", "Kathy", "Edward", "Margaret", "Jeffrey", "Janice", "George", "Carolyn", "Gregory", "Denise", "Kevin", "Judy", "Douglas", "Rebecca", "Terry", "Joyce", "Anthony", "Teresa", "Jerry", "Christine", "Bruce", "Catherine", "Randy", "Shirley", "Brian", "Judith", "Frank", "Betty", "Scott", "Beverly", "Roger", "Lisa", "Raymond", "Laura", "Peter", "Theresa", "Patrick", "Connie", "Keith", "Ann", "Lawrence", "Gloria", "Wayne", "Julie", "Danny", "Gail", "Alan", "Joan", "Gerald", "Paula", "Ricky", "Peggy", "Carl", "Cindy", "Christopher", "Martha", "Dale", "Bonnie", "Walter", "Jane", "Craig", "Cathy", "Willie", "Robin", "Johnny", "Debbie", "Arthur", "Diana", "Steve", "Marilyn", "Joe", "Kathryn", "Randall", "Dorothy", "Russell", "Wanda", "Jack", "Jean", "Henry", "Vicki", "Harold", "Sheila", "Roy", "Virginia", "Andrew", "Sherry", "Philip", "Katherine", "Ralph", "Rose", "Billy", "Lynn", "Glenn", "Jo", "Stanley", "Ruth", "Jimmy", "Maria", "Rodney", "Darlene", "Barry", "Jacqueline", "Samuel", "Rita", "Eric", "Rhonda", "Bobby", "Phyllis", "Albert", "Helen", "Phillip", "Vickie", "Ronnie", "Kim", "Martin", "Lori", "Mike", "Ellen", "Eugene", "Elaine", "Louis", "Joanne", "Howard", "Anne", "Allen", "Valerie", "Curtis", "Alice", "Jeffery", "Frances", "Frederick", "Suzanne", "Leonard", "Marie", "Harry", "Victoria", "Micheal", "Kimberly", "Tony", "Anita", "Ernest", "Laurie", "Eddie", "Michelle", "Fred", "Sally", "Darrell", "Terri", "Jay", "Marcia", "Melvin", "Terry", "Lee", "Jennifer", "Matthew", "Leslie", "Vincent", "Doris", "Tommy", "Maureen", "Francis", "Wendy", "Marvin", "Michele", "Dean", "Anna", "Rick", "Marsha", "Victor", "Angela", "Norman", "Sarah", "Earl", "Sylvia", "Jose", "Jill", "Calvin", "Dawn", "Ray", "Sue", "Clifford", "Evelyn", "Alfred", "Roberta", "Jerome", "Jeanne", "Bradley", "Charlotte", "Clarence", "Eileen", "Don", "Lois", "Theodore", "Colleen", "Jon", "Stephanie", "Rickey", "Annette", "Joel", "Glenda", "Jesse", "Yvonne", "Jim", "Dianne", "Edwin", "Tina", "Dan", "Beth", "Chris", "Lorraine", "Bernard", "Constance", "Jonathan", "Renee", "Gordon", "Charlene", "Glen", "Joann", "Jeff", "Julia", "Warren", "Gwendolyn", "Leroy", "Norma", "Herbert", "Regina", "Duane", "Amy", "Bill", "Loretta", "Benjamin", "Sheryl", "Tom", "Carla", "Alvin", "Andrea", "Nicholas", "Tammy", "Tim", "Irene", "Mitchell", "Jan", "Marc", "Louise", "Leslie", "Juanita", "Leon", "Marlene", "Kim", "Penny", "Dwight", "Rosemary", "Bryan", "Becky", "Lloyd", "Kay", "Vernon", "Joy", "Gene", "Geraldine", "Reginald", "Jeanette", "Lonnie", "Gayle", "Guy", "Annie", "Gilbert", "Vivian", "Garry", "Claudia", "Juan", "Lynda", "Karl", "Melissa", "Kent", "Audrey", "Kurt", "Lynne", "Todd", "Patsy", "Jackie", "Melinda", "Greg", "Vicky", "Lewis", "Toni", "Wesley", "June", "Clyde", "Belinda", "Floyd", "Marjorie", "Neil", "Arlene", "Allan", "Patti", "Donnie", "Ruby", "Perry", "Sara", "Lester", "Rosa", "Brad", "Melanie", "Manuel", "Christina", "Kirk", "Delores", "Carlos", "Jackie", "Jimmie", "Vanessa", "Leo", "Carmen", "Randolph", "Monica", "Charlie", "Janis", "Robin", "Holly", "Dana", "Marianne", "Darryl", "Dolores", "Dave", "Shelley", "Ted", "Veronica", "Milton", "Mildred", "Daryl", "Eva", "Kerry", "Dana", "Freddie", "Rachel", "Brent", "Shelia", "Harvey", "Roxanne", "Gerard", "Carole", "Stuart", "Lillian", "Johnnie", "Josephine", "Herman", "Carrie", "Lynn", "Patty", "Rex", "Sherri", "Arnold",
}
);

    static readonly ReadOnlyCollection<string> firstNames1960s = new ReadOnlyCollection<string>(
new string[] {
        "Michael", "Lisa", "David", "Mary", "John", "Susan", "James", "Karen", "Robert", "Kimberly", "Mark", "Patricia", "William", "Linda", "Richard", "Donna", "Thomas", "Michelle", "Jeffrey", "Cynthia", "Steven", "Sandra", "Joseph", "Deborah", "Timothy", "Tammy", "Kevin", "Pamela", "Scott", "Lori", "Brian", "Laura", "Charles", "Elizabeth", "Paul", "Julie", "Daniel", "Brenda", "Christopher", "Jennifer", "Kenneth", "Barbara", "Anthony", "Angela", "Gregory", "Sharon", "Ronald", "Debra", "Donald", "Teresa", "Gary", "Nancy", "Stephen", "Christine", "Eric", "Cheryl", "Edward", "Denise", "Douglas", "Kelly", "Todd", "Tina", "Patrick", "Kathleen", "George", "Melissa", "Keith", "Robin", "Larry", "Amy", "Matthew", "Diane", "Terry", "Dawn", "Andrew", "Carol", "Dennis", "Tracy", "Randy", "Kathy", "Jerry", "Rebecca", "Peter", "Theresa", "Frank", "Kim", "Craig", "Rhonda", "Raymond", "Stephanie", "Jeffery", "Cindy", "Bruce", "Janet", "Rodney", "Wendy", "Mike", "Maria", "Roger", "Michele", "Tony", "Jacqueline", "Ricky", "Debbie", "Steve", "Margaret", "Jeff", "Paula", "Troy", "Sherry", "Alan", "Catherine", "Carl", "Carolyn", "Danny", "Laurie", "Russell", "Sheila", "Chris", "Ann", "Bryan", "Jill", "Gerald", "Connie", "Wayne", "Diana", "Joe", "Terri", "Randall", "Suzanne", "Lawrence", "Andrea", "Dale", "Beth", "Phillip", "Janice", "Johnny", "Valerie", "Vincent", "Renee", "Martin", "Leslie", "Bradley", "Christina", "Billy", "Gina", "Glenn", "Lynn", "Shawn", "Annette", "Jonathan", "Cathy", "Jimmy", "Katherine", "Sean", "Judy", "Curtis", "Carla", "Barry", "Wanda", "Bobby", "Anne", "Walter", "Dana", "Jon", "Joyce", "Philip", "Regina", "Samuel", "Beverly", "Jay", "Monica", "Jason", "Bonnie", "Dean", "Kathryn", "Jose", "Anita", "Tim", "Sarah", "Roy", "Darlene", "Willie", "Jane", "Arthur", "Sherri", "Darryl", "Martha", "Henry", "Anna", "Darrell", "Colleen", "Allen", "Vicki", "Victor", "Tracey", "Harold", "Judith", "Greg", "Tamara", "Albert", "Gloria", "Jack", "Betty", "Darren", "Stacey", "Ronnie", "Penny", "Ralph", "Shirley", "Joel", "Victoria", "Louis", "Jean", "Jim", "Peggy", "Micheal", "Melanie", "Marc", "Joan", "Frederick", "Melinda", "Eddie", "Shelly", "Lee", "Stacy", "Stanley", "Virginia", "Tommy", "Marie", "Eugene", "Maureen", "Tom", "Ruth", "Tracy", "Julia", "Howard", "Ellen", "Leonard", "Tonya", "Kurt", "Shannon", "Marvin", "Heidi", "Kelly", "Joanne", "Brent", "Dorothy", "Ernest", "Rita", "Dwayne", "Gail", "Aaron", "Heather", "Brett", "Deanna", "Rick", "Holly", "Benjamin", "Rose", "Bill", "Vickie", "Reginald", "Carrie", "Duane", "Veronica", "Juan", "Yvonne", "Fred", "Becky", "Melvin", "Helen", "Adam", "Sylvia", "Norman", "Yolanda", "Dan", "April", "Mitchell", "Terry", "Harry", "Elaine", "Jesse", "Sheri", "Carlos", "Marilyn", "Nicholas", "Alice", "Jerome", "Jodi", "Kirk", "Rachel", "Ray", "Sheryl", "Don", "Jackie", "Calvin", "Phyllis", "Glen", "Jamie", "Brad", "Frances", "Theodore", "Crystal", "Derrick", "Joann", "Karl", "Eileen", "Edwin", "Shelley", "Earl", "Toni", "Lance", "Charlene", "Francis", "Sally", "Kent", "Charlotte", "Derek", "Kristine", "Wesley", "Jeanne", "Alfred", "Sara", "Antonio", "Tanya", "Warren", "Belinda", "Andre", "Carmen", "Clarence", "Sandy", "Bernard", "Evelyn", "Kyle", "Alicia", "Tyrone", "Sonya", "Manuel", "Lorraine", "Chad", "Jeanette", "Luis", "Yvette", "Gordon", "Loretta", "Dave", "Joy", "Nathan", "Sue", "Guy", "Norma", "Kerry", "Roberta", "Daryl", "Vanessa", "Leroy", "Shari", "Lonnie", "Jo", "Perry", "Natalie", "Erik", "Tammie", "Maurice", "Traci", "Marcus", "Gwendolyn", "Alvin", "Nicole", "Gilbert", "Felicia", "Vernon", "Melody", "Alexander", "Tami", "Stuart", "Shelia", "Rickey", "Marcia", "Shane", "Doris", "Franklin", "Kristen", "Leon", "Audrey", "Gregg", "Karla", "Bob", "Jody", "Darin", "Glenda", "Leslie", "Patty", "Herbert", "Amanda", "Gene", "Pam"
}
);

    static readonly ReadOnlyCollection<string> firstNames1970s = new ReadOnlyCollection<string>(
new string[] {
        "Michael","Jennifer","Christopher","Amy","Jason","Melissa","David","Michelle","James","Kimberly","John","Lisa","Robert","Angela","Brian","Heather","William","Stephanie","Matthew","Nicole","Joseph","Jessica","Daniel","Elizabeth","Kevin","Rebecca","Eric","Kelly","Jeffrey","Mary","Richard","Christina","Scott","Amanda","Mark","Julie","Steven","Sarah","Thomas","Laura","Timothy","Shannon","Anthony","Christine","Charles","Tammy","Joshua","Tracy","Ryan","Karen","Jeremy","Dawn","Paul","Susan","Andrew","Andrea","Gregory","Tina","Chad","Patricia","Kenneth","Cynthia","Jonathan","Lori","Stephen","Rachel","Shawn","April","Aaron","Maria","Adam","Wendy","Patrick","Crystal","Justin","Stacy","Sean","Erin","Edward","Jamie","Todd","Carrie","Donald","Tiffany","Ronald","Tara","Benjamin","Sandra","Keith","Monica","Bryan","Danielle","Gary","Stacey","Jose","Pamela","Nathan","Tonya","Douglas","Sara","Nicholas","Michele","Brandon","Teresa","George","Denise","Travis","Jill","Peter","Katherine","Craig","Melanie","Bradley","Dana","Larry","Holly","Dennis","Erica","Shane","Brenda","Raymond","Deborah","Troy","Tanya","Jerry","Sharon","Samuel","Donna","Frank","Amber","Jesse","Emily","Jeffery","Robin","Juan","Linda","Terry","Kathleen","Corey","Leslie","Phillip","Christy","Marcus","Kristen","Derek","Catherine","Rodney","Kristin","Joel","Misty","Carlos","Barbara","Randy","Heidi","Jacob","Nancy","Jamie","Cheryl","Tony","Theresa","Russell","Brandy","Brent","Alicia","Antonio","Veronica","Billy","Gina","Derrick","Jacqueline","Kyle","Rhonda","Erik","Anna","Johnny","Renee","Marc","Megan","Carl","Tamara","Philip","Melinda","Roger","Kathryn","Bobby","Debra","Brett","Sherry","Danny","Allison","Curtis","Valerie","Jon","Diana","Vincent","Paula","Cory","Kristina","Jimmy","Ann","Victor","Margaret","Lawrence","Victoria","Dustin","Cindy","Gerald","Jodi","Walter","Natalie","Alexander","Brandi","Joe","Kristi","Christian","Suzanne","Samantha","Alan","Beth","Shannon","Tracey","Wayne","Regina","Jared","Vanessa","Gabriel","Kristy","Martin","Misty","Jay","Yolanda","Willie","Deanna","Luis","Carla","Micheal","Sheila","Henry","Laurie","Wesley","Anne","Randall","Shelly","Brad","Diane","Darren","Sabrina","Roy","Janet","Albert","Erika","Arthur","Katrina","Ricky","Courtney","Lance","Colleen","Lee","Julia","Andre","Jenny","Bruce","Jaime","Mario","Kathy","Frederick","Felicia","Louis","Alison","Darrell","Lauren","Damon","Kelli","Shaun","Leah","Nathaniel","Ashley","Zachary","Kim","Casey","Traci","Adrian","Kristine","Jesus","Tricia","Jeremiah","Joy","Jack","Krista","Ronnie","Kara","Dale","Terri","Tyrone","Sonya","Manuel","Aimee","Ricardo","Natasha","Harold","Cassandra","Kelly","Bridget","Barry","Anita","Ian","Kari","Reginald","Nichole","Glenn","Christie","Ernest","Marie","Steve","Virginia","Seth","Connie","Eugene","Martha","Clinton","Carmen","Miguel","Stacie","Tommy","Lynn","Eddie","Katie","Leonard","Monique","Maurice","Kristie","Roberto","Shelley","Dwayne","Sherri","Jerome","Angel","Ralph","Bonnie","Marvin","Mandy","Jorge","Jody","Francisco","Shawna","Neil","Kerry","Alex","Annette","Dean","Yvonne","Kristopher","Toni","Calvin","Meredith","Kurt","Molly","Theodore","Kendra","Ruben","Joanna","Jermaine","Sonia","Tracy","Janice","Edwin","Robyn","Stanley","Brooke","Melvin","Kerri","Howard","Sheri","Mitchell","Becky","Duane","Gloria","Trevor","Mindy","Jeff","Tracie","Geoffrey","Angie","Hector","Kellie","Terrence","Claudia","Terrance","Ruth","Oscar","Wanda","Jaime","Jeanette","Clifford","Cathy","Harry","Adrienne"
}
);

    static readonly ReadOnlyCollection<string> lastNames = new ReadOnlyCollection<string>(
      new string[] {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker", "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy", "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey", "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson", "Watson", "Brooks", "Chavez", "Wood", "James", "Bennett", "Gray", "Mendoza", "Ruiz", "Hughes", "Price", "Alvarez", "Castillo", "Sanders", "Patel", "Myers", "Long", "Ross", "Foster", "Jimenez", "Powell", "Jenkins", "Perry", "Russell", "Sullivan", "Bell", "Coleman", "Butler", "Henderson", "Barnes", "Gonzales", "Fisher", "Vasquez", "Simmons", "Romero", "Jordan", "Patterson", "Alexander", "Hamilton", "Graham", "Reynolds", "Griffin", "Wallace", "Moreno", "West", "Cole", "Hayes", "Bryant", "Herrera", "Gibson", "Ellis", "Tran", "Medina", "Aguilar", "Stevens", "Murray", "Ford", "Castro", "Marshall", "Owens", "Harrison", "Fernandez", "McDonald", "Woods", "Washington", "Kennedy", "Wells", "Vargas", "Henry", "Chen", "Freeman", "Webb", "Tucker", "Guzman", "Burns", "Crawford", "Olson", "Simpson", "Porter", "Hunter", "Gordon", "Mendez", "Silva", "Shaw", "Snyder", "Mason", "Dixon", "Munoz", "Hunt", "Hicks", "Holmes", "Palmer", "Wagner", "Black", "Robertson", "Boyd", "Rose", "Stone", "Salazar", "Fox", "Warren", "Mills", "Meyer", "Rice", "Schmidt", "Garza", "Daniels", "Ferguson", "Nichols", "Stephens", "Soto", "Weaver", "Ryan", "Gardner", "Payne", "Grant", "Dunn", "Kelley", "Spencer", "Hawkins", "Arnold", "Pierce", "Vazquez", "Hansen", "Peters", "Santos", "Hart", "Bradley", "Knight", "Elliott", "Cunningham", "Duncan", "Armstrong", "Hudson", "Carroll", "Lane", "Riley", "Andrews", "Alvarado", "Ray", "Delgado", "Berry", "Perkins", "Hoffman", "Johnston", "Matthews", "Pena", "Richards", "Contreras", "Willis", "Carpenter", "Lawrence", "Sandoval", "Guerrero", "George", "Chapman", "Rios", "Estrada", "Ortega", "Watkins", "Greene", "Nunez", "Wheeler", "Valdez", "Harper", "Burke", "Larson", "Santiago", "Maldonado", "Morrison", "Franklin", "Carlson", "Austin", "Dominguez", "Carr", "Lawson", "Jacobs", "OBrien", "Lynch", "Singh", "Vega", "Bishop", "Montgomery", "Oliver", "Jensen", "Harvey", "Williamson", "Gilbert", "Dean", "Sims", "Espinoza", "Howell", "Li", "Wong", "Reid", "Hanson", "Le", "McCoy", "Garrett", "Burton", "Fuller", "Wang", "Weber", "Welch", "Rojas", "Lucas", "Marquez", "Fields", "Park", "Yang", "Little", "Banks", "Padilla", "Day", "Walsh", "Bowman", "Schultz", "Luna", "Fowler", "Mejia", "Davidson", "Acosta", "Brewer", "May", "Holland", "Juarez", "Newman", "Pearson", "Curtis", "Corona", "Douglas", "Schneider", "Joseph", "Barrett", "Navarro", "Figueroa", "Keller", "Avila", "Wade", "Molina", "Stanley", "Hopkins", "Campos", "Barnett", "Bates", "Chambers", "Caldwell", "Beck", "Lambert", "Miranda", "Byrd", "Craig", "Ayers", "Lowe", "Frazier", "Powers", "Neal", "Leonard", "Gregory", "Carrillo", "Sutton", "Fleming", "Rhodes", "Shelton", "Schwartz", "Norris", "Jennings", "Watts", "Duran", "Walters", "Cohen", "McDaniel", "Moran", "Parks", "Steele", "Vaughn", "Becker", "Holt", "Deleon", "Barker", "Terry", "Hale", "Leon", "Hail", "Benson", "Haynes", "Horton", "Miles", "Lyons", "Pham", "Graves", "Bush", "Thornton", "Wolfe", "Warner", "Cabrera", "McKinney", "Mann", "Zimmerman", "Dawson", "Lara", "Fletcher", "Page", "McCarthy", "Love", "Robles", "Cervantes", "Solis", "Erickson", "Reeves", "Chang", "Klein", "Salinas", "Fuentes", "Baldwin", "Daniel", "Simon", "Velasquez", "Hardy", "Higgins", "Aguirre", "Lin", "Cummings", "Chandler", "Sharp", "Barber", "Bowen", "Ochoa", "Dennis", "Robbins", "Liu", "Ramsey", "Francis", "Griffith", "Paul", "Blair", "OConnor", "Cardenas", "Pacheco", "Cross", "Calderon", "Quinn", "Moss", "Swanson", "Chan", "Rivas", "Khan", "Rodgers", "Serrano", "Fitzgerald", "Rosales", "Stevenson", "Christensen", "Manning", "Gill", "Curry", "McLaughlin", "Harmon", "McGee", "Gross", "Doyle", "Garner", "Newton", "Burgess", "Reese", "Walton", "Blake", "Trujillo", "Adkins", "Brady", "Goodman", "Roman", "Webster", "Goodwin", "Fischer", "Huang", "Potter", "Delacruz", "Montoya", "Todd", "Wu", "Hines", "Mullins", "Castaneda", "Malone", "Cannon", "Tate", "Mack", "Sherman", "Hubbard", "Hodges", "Zhang", "Guerra", "Wolf", "Valencia", "Saunders", "Franco", "Rowe", "Gallagher", "Farmer", "Hammond", "Hampton", "Townsend", "Ingram", "Wise", "Gallegos", "Clarke", "Barton", "Schroeder", "Maxwell", "Waters", "Logan", "Camacho", "Strickland", "Norman", "Person", "Colon", "Parsons", "Frank", "Harrington", "Glover", "Osborne", "Buchanan", "Casey", "Floyd", "Patton", "Ibarra", "Ball", "Tyler", "Suarez", "Bowers", "Orozco", "Salas", "Cobb", "Gibbs", "Andrade", "Bauer", "Conner", "Moody", "Escobar", "McGuire", "Lloyd", "Mueller", "Hartman", "French", "Kramer", "McBride", "Pope", "Lindsey", "Velazquez", "Norton", "McCormick", "Sparks", "Flynn", "Yates", "Hogan", "Marsh", "Macias", "Villanueva", "Zamora", "Pratt", "Stokes", "Owen", "Ballard", "Lang", "Brock", "Villarreal", "Charles", "Drake", "Barrera", "Cain", "Patrick", "Pineda", "Burnett", "Mercado", "Santana", "Shepherd", "Bautista", "Ali", "Shaffer", "Lamb", "Trevino", "McKenzie", "Hess", "Beil", "Olsen", "Cochran", "Morton", "Nash", "Wilkins", "Petersen", "Briggs", "Shah", "Roth", "Nicholson", "Holloway", "Lozano", "Rangel", "Flowers", "Hoover", "Short", "Arias", "Mora", "Valenzuela", "Bryan", "Meyers", "Weiss", "Underwood", "Bass", "Greer", "Summers", "Houston", "Carson", "Morrow", "Clayton", "Whitaker", "Decker", "Yoder", "Collier", "Zuniga", "Carey", "Wilcox", "Melendez", "Poole", "Roberson", "Larsen", "Conley", "Davenport", "Copeland", "Massey", "Lam", "Huff", "Rocha", "Cameron", "Jefferson", "Hood", "Monroe", "Anthony", "Pittman", "Huynh", "Randall", "Singleton", "Kirk", "Combs", "Mathis", "Christian", "Skinner", "Bradford", "Richard", "Galvan", "Wall", "Boone", "Kirby", "Wilkinson", "Bridges", "Bruce", "Atkinson", "Velez", "Meza", "Roy", "Vincent", "York", "Hodge", "Villa", "Abbott", "Allison", "Tapia", "Gates", "Chase", "Sosa", "Sweeney", "Farrell", "Wyatt", "Dalton", "Horn", "Barron", "Phelps", "Yu", "Dickerson", "Heath", "Foley", "Atkins", "Mathews", "Bonilla", "Acevedo", "Benitez", "Zavala", "Hensley", "Glenn", "Cisneros", "Harrell", "Shields", "Rubio", "Huffman", "Choi", "Boyer", "Garrison", "Arroyo", "Bond", "Kane", "Hancock", "Callahan", "Dillon", "Cline", "Wiggins", "Grimes", "Arellano", "Melton", "ONeill", "Savage", "Ho", "Beltran", "Pitts", "Parish", "Ponce", "Rich", "Booth", "Koch", "Golden", "Ware", "Brennan", "McDowell", "Marks", "Cantu", "Humphrey", "Baxter", "Sawyer", "Clay", "Tanner", "Hutchinson", "Kaur", "Berg", "Wiley", "Gilmore", "Russo", "Villegas", "Hobbs", "Keith", "Wilkerson", "Ahmed", "Beard", "McClain", "Montes", "Mata", "Rosario", "Vang", "Walter", "Henson", "ONeal", "Mosley", "McClure", "Beasley", "Stephenson", "Snow", "Huerta", "Preston", "Vance", "Barry", "Johns", "Eaton", "Blackwell", "Dyer", "Prince", "Macdonald", "Solomon", "Guevara", "Stafford", "English", "Hurst", "Woodard", "Cortes", "Shannon", "Kemp", "Nolan", "McCullough", "Merritt", "Murillo", "Moon", "Salgado", "Strong", "Kline", "Cordova", "Barajas", "Roach", "Rosas", "Winters", "Jacobson", "Lester", "Knox", "Bullock", "Kerr", "Leach", "Meadows", "Orr", "Davila", "Whitehead", "Pruitt", "Kent", "Conway", "McKee", "Barr", "David", "Dejesus", "Marin", "Berger", "McIntyre", "Blankenship", "Gaines", "Palacios", "Cuevas", "Bartlett", "Durham", "Dorsey", "McCall", "ODonnell", "Stein", "Browning", "Stout", "Lowery", "Sloan", "McLean", "Hendricks", "Calhoun", "Sexton", "Chung", "Gentry", "Hull", "Duarte", "Ellison", "Nielsen", "Gillespie", "Buck", "Middleton", "Sellers", "LeBlanc", "Esparza", "Hardin", "Bradshaw", "McIntosh", "Howe", "Livingston", "Frost", "Glass", "Morse", "Knapp", "Herman", "Stark", "Bravo", "Noble", "Spears", "Weeks", "Corona", "Frederick", "Buckley", "McFarland", "Hebert", "Enriquez", "Hickman", "Quintero", "Randolph", "Schaefer", "Walls", "Trejo", "House", "Reilly", "Pennington", "Michael", "Conrad", "Giles", "Benjamin", "Crosby", "Fitzpatrick", "Donovan", "Mays", "Mahoney", "Valentine", "Raymond", "Medrano", "Hahn", "McMillan", "Small", "Bentley", "Felix", "Peck", "Lucero", "Boyle", "Hanna", "Pace", "Rush", "Hurley", "Harding", "McConnell", "Bernal", "Nava", "Ayers", "Everett", "Ventura", "Avery", "Pugh", "Mayer", "Bender", "Shepard", "McMahon", "Landry", "Case", "Sampson", "Moses", "Magana", "Blackburn", "Dunlap", "Gould", "Duffy", "Vaughan", "Herring", "McKay", "Espinosa", "Rivers", "Farley", "Bernard", "Ashley", "Friedman", "Potts", "Truong", "Costa", "Correa", "Blevins", "Nixon", "Clements", "Fry", "Delarosa", "Best", "Benton", "Lugo", "Portillo", "Dougherty", "Crane", "Haley", "Phan", "Villalobos", "Blanchard", "Horne", "Finley", "Quintana", "Lynn", "Esquivel", "Bean", "Dodson", "Mullen", "Xiong", "Hayden", "Cano", "Levy", "Huber", "Richmond", "Moyer", "Lim", "Frye", "Sheppard", "McCarty", "Avalos", "Booker", "Waller", "Parra", "Woodward", "Jaramillo", "Krueger", "Rasmussen", "Brandt", "Peralta", "Donaldson", "Stuart", "Faulkner", "Maynard", "Galindo", "Coffey", "Estes", "Sanford", "Burch", "Maddox", "Vo", "OConnell", "Vu", "Andersen", "Spence", "McPherson", "Church", "Schmitt", "Stanton", "Leal", "Cherry", "Compton", "Dudley", "Sierra", "Pollard", "Alfaro", "Hester", "Proctor", "Lu", "Hinton", "Novak", "Good", "Madden", "McCann", "Terrell", "Jarvis", "Dickson", "Reyna", "Cantrell", "Mayo", "Branch", "Hendrix", "Rollins", "Rowland", "Whitney", "Duke", "Odom", "Daugherty", "Travis", "Tang", "Archer"
      }
    );

    static readonly ReadOnlyCollection<string> placeNouns = new ReadOnlyCollection<string>(
     new string[] {
        "Band",
        "Pizza",
        "Family",
        "Lemonade",
        "Breakfast",
        "Lunch",
        "Brunch",
        "Dinner",
        "Supper",
        "Dessert",
        "Cake",
        "Coffee",
        "Tea",
        "Milk",
        "Pancake",
        "Burger",
        "Dairy",
        "Hot-Dog",
        "Fiesta",
        "Seafood",
        "Fried Chicken",
        "Fry",
        "Fries",
        "Hamburgers",
        "Donut",
        "Sub",
        "Submarine",
        "Ice-Cream",
        "Smoothie",
        "Taco",
        "Sandwiches",
        "Shakes",
        "Soda",
        "Drinks",
        "Concert",
        "Guys",
        "Gals",
        "Pinball",
        "Sports",
        "Fast Food",
     }
   );

    static readonly ReadOnlyCollection<string> placeAdjective = new ReadOnlyCollection<string>(
     new string[] {
        "Time",
        "Mystery",
        "Fresh",
        "Cooked",
        "Fun",
        "Play",
        "Wacky",
        "Odyssey",
        "American",
        "All-American",
        "Freedom",
        "King",
        "Queen",
        "Rockabilly",
        "Rockin'",
        "Swingin'",
        "Country",
        "Cowboy",
        "Disaster",
        "Hollywood",
        "Classic",
        "Scrumptious",
        "Sunny",
        "Magic",
        "Magical",
        "Fantasy",
        "Chinese",
        "Mexican",
        "Italian",
        "Thai",
        "Indian",
        "Japanese",
        "Korean",
        "Greek",
        "Vietnamese",
        "Cuban",
        "English",
        "German",
        "French",
        "Dutch",
        "Danish",
        "Egyptian",
        "Finnish",
        "Italian",
        "Icelandic",
        "Russian",
        "Swedish",
        "Swiss",
        "Taiwanese",
        "Turkish",
        "Canadian",
        "Australian",
        "Brazilian",
        "Famous",
        "All-Famous",
        "All-Star",
        "5-Star",
        "National",
        "International",
        "Southern",
        "Northern",
        "Mid-Western",
        "Eastern",
        "Western",
        "Coastline",
        "Bayside",
        "Funny",
        "Express",
        "Kentucky",
        "California",
        "Style",
        "New York",
        "Texas",
        "Texan",
        "Five",
        "Four",
        "Three",
        "Two",
        "One-Stop",
        "Fat",
        "Greasy",
        "Fancy",
        "Cheap",
        "Outdoor",
        "Indoor",
        "Outback",
        "Vegetarian",
        "Vegan",
        "Religious",
        "Exotic",
        "Mouth-Waterin'",
        "Jaw-Droppin'",
        "Vegas",
        "Poor",
        "Unsafe",
        "Rickety",
        "Pretty",
        "Neighborhood",
        "Local",
        "One-of-a-Kind",
        "Headlining",
        "As-Seen-On-TV",
        "Redneck",
        "Daytime",
        "Nighttime",
        "Romantic",
        "24 Hour",
        "All-Day",
        "24/7",
        "Nonstop",
        "Cajun",
        "Spicy",
        "Hot",
        "Cold",
        "All-You-Can-Eat",
        "Creepy",
        "Spooky",
        "Happy",
        "Nice",
        "Quaint",
        "Little",
     }
   );

    static readonly ReadOnlyCollection<string> placePlaces = new ReadOnlyCollection<string>(
     new string[] {
        "Place",
        "Emporium",
        "Shop",
        "Theatre",
        "World",
        "Zone",
        "Dry Cleaner",
        "Palace",
        "House",
        "Castle",
        "Market",
        "Stand",
        "Shack",
        "Diner",
        "Restaurant",
        "Liquor Store",
        "Kitchen",
        "Steakhouse",
        "Bar",
        "Penny Arcade",
        "Arcade",
        "Cafe",
        "Festival",
        "Hut",
        "Barbecue",
        "BBQ",
        "Joint",
        "Grill",
        "Tavern",
        "Club",
        "Fountain",
        "Corner-Store",
        "Wonderland",
        "Playhouse",
        "Center",
        "Alley",
        "Bakery",
        "Company",
        "Drive-In",
        "Drive-Thru",
        "Creamery",
        "Pizzeria",
        "Factory",
        "Corral",
        "Barn",
        "Rest-Stop",
        "Restrooms",
        "Casino",
        "Experience",
        "Plaza",
        "Land",
        "Buffet",
        "Sports Bar",
        "Eatery",
        "Town",
     }
   );
}
