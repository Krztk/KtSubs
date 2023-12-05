namespace KtSubs.Core.Selection
{
    public class Pattern
    {
        private readonly string pattern;

        public Pattern(string pattern)
        {
            this.pattern = pattern;
        }

        public HashSet<ISelection> ToSelections(int maxNumber)
        {
            var selections = new HashSet<ISelection>();
            var entryPattern = pattern.Split(',');
            foreach (var wordPattern in entryPattern)
            {
                if (wordPattern.IndexOf('-') != -1)
                {
                    var rangeValues = wordPattern.Split('-');
                    if (rangeValues.Length != 2)
                        continue;

                    bool multipleSingleSelections = rangeValues[1].Trim().EndsWith('*');
                    rangeValues[1] = rangeValues[1].Replace("*", "");

                    if (int.TryParse(rangeValues[0], out int left) && int.TryParse(rangeValues[1], out int right))
                    {
                        if (left == 0) //0-5 -> 1-5; 0-0 -> 1-0; 0-1 -> 1-1
                            left = 1;

                        if (left > maxNumber)
                            continue;

                        if (left > right)
                            continue;

                        if (multipleSingleSelections)
                        {
                            for (int i = left; i <= Math.Min(right, maxNumber); i++)
                            {
                                selections.Add(new Selection(i));
                            }
                        }
                        else
                        {
                            if (left == right)
                            {
                                selections.Add(new Selection(left));
                            }
                            else
                            {
                                selections.Add(new MergedSelection(left, Math.Min(right, maxNumber)));
                            }
                        }
                    }
                }
                else
                {
                    if (int.TryParse(wordPattern, out int number))
                    {
                        if (number > 0 && number <= maxNumber)
                        {
                            selections.Add(new Selection(number));
                        }
                    }
                }
            }

            return selections;
        }
    }
}