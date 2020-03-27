
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace LFSmodel
{
    class AhpProcess
    {
        List<double> criteria_priority;
        List<double> consistency_weighted_sum;

        //Pairwise comparison
        public Matrix<double> PairwiseComparison(int row, int col, double[] weights)
        {
            Matrix<double> pairwise_matrix = Matrix<double>.Build.DenseDiagonal(row, col,1);
            var count = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (i > j)
                    {
                        pairwise_matrix[j, i] = weights[count];
                        pairwise_matrix[i, j] = 1 / weights[count];
                        count++;
                    }
                }
            }

            return pairwise_matrix;
        }


        //Sum up values in each columns
        public List<double> SumUpColumns(Matrix<double> pwMatrix, int row, int col)
        {
            List<double> column_total = new List<double>();
            double sum = 0;
            int counter = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sum += pwMatrix[j,i];

                }

                column_total.Add(sum);
                counter++;
                sum = 0;

            }

            return column_total;
        }

        //Normalized matrix - Divide each element in every column by the sum of that column
        public Matrix<double> NormalizeMatrix(Matrix<double> matrix_pw, int row, int col, List<double> column_total)
        {
            Matrix<double> normalized_matrix = Matrix<double>.Build.Dense(matrix_pw.ColumnCount, matrix_pw.RowCount);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    normalized_matrix[j, i] = (Convert.ToDouble(matrix_pw[j, i])) / column_total[i];
                }
            }

            return normalized_matrix;
        }

        //Get priorities for each criteria - Sum up values in each columns
        public List<double> GetCriteriaWeight(Matrix<double> normalisedMatrix, int row, int col)
        {
            criteria_priority = new List<double>();
            double sum = 0;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sum += (Convert.ToDouble(normalisedMatrix[i,j]));

                }
                criteria_priority.Add(sum / col);
                sum = 0;
            }

            return criteria_priority;
        }

        //Get consistency weighted matrix - Multiply values of each columns with its corressponding prorities
        public Matrix<double> GetConsistencyWeightMatrix(Matrix<double> matrix_pw, int row, int col, List<double> criteriaWeights)
        {

            Matrix<double> consistency_weight_matrix = Matrix<double>.Build.Dense(matrix_pw.ColumnCount, matrix_pw.RowCount);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    consistency_weight_matrix[j, i] = (Convert.ToDouble(matrix_pw[j, i])) * criteriaWeights[i];

                }
                
            }
            return consistency_weight_matrix;
        }

        //Get consistency weighted sum - Multiply values of each columns with its corressponding prorities
        public List<double> GetConsistencyWeightedSum(Matrix<double> consistency_weight_matrix, int row, int col)
        {
            consistency_weighted_sum = new List<double>();
            double sum = 0;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sum += (Convert.ToDouble(consistency_weight_matrix[i, j]));

                }
                consistency_weighted_sum.Add(sum);
                sum = 0;
            }
   
            return consistency_weighted_sum;
        }

        //Get consistency ratio - Divide the consistency weighted sum of each criteria with priorities of each criteria
        public double GetConsistencyRatio(List<double> consitencyWeightsSum, List<double> criteriaWeights, int col)
        {
            double sum = 0;
            double lambdaMax = 0;
            double consistencyIndex = 0;
            double randomMatrixConsIndex = 1.35;
            double consistencyRatio = 0;

            for (int i = 0; i < col; i++)
            {
                    sum += (Convert.ToDouble(consitencyWeightsSum[i] / criteriaWeights[i]));
            }
            lambdaMax = sum / col;
            consistencyIndex = (lambdaMax - col) / (col - 1);
            consistencyRatio = consistencyIndex / randomMatrixConsIndex;
            return consistencyRatio;
        }

        //Pairwise comparison for alternatives
        public Matrix<double> PairwiseComparisonForAlternatives(IList<double> criteriaBins)
        {
            int networkCount = criteriaBins.Count;
            Matrix<double> alternative_pairwise_matrix = Matrix<double>.Build.DenseDiagonal(networkCount, networkCount, 1);

            for (int i = 0; i < networkCount; i++)
            {
                for (int j = 0; j < networkCount; j++)
                {
                    if (i > j)
                    {
                        int diff = (int)criteriaBins[j] - (int)criteriaBins[i];
                        int diffInWeights = System.Math.Abs(diff);
                        int weight = diffInWeights + 1;


                        bool positive = diff > 0;

                        if (positive)
                        {
                            alternative_pairwise_matrix[j, i] = weight;
                            alternative_pairwise_matrix[i, j] = (Convert.ToDouble(1)) / weight;

                        }
                        else
                        {
                            alternative_pairwise_matrix[j, i] = (Convert.ToDouble(1)) / weight;
                            alternative_pairwise_matrix[i, j] = weight;

                        }
                    }
                }
            }

            return alternative_pairwise_matrix;
        }

        //1.Find min and max values of the networks for each criteria Createria.
        //2.The values between min and max are filled up in 9 different bins(to find weights of each network) exponentially
        //3.Total networks are iterated to find each network falls in which bin to get the priorities of each network
        public IList<double> Bins(IList<double> criteriaList, IList<NetworkEntity> criteriaSortedList, String criteria)
        {
            double minValue = 0;
            double maxValue = 0;

            if (criteria == "trailLength")
            {
                 minValue = criteriaSortedList[0].trail_length;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].trail_length;
            }else if (criteria == "trailDensity")
            {
                 minValue = criteriaSortedList[0].trail_density;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].trail_density;
            }
            else if (criteria == "crossCountryDist")
            {
                 minValue = criteriaSortedList[0].cross_country_transport_dist;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].cross_country_transport_dist;
            }
            else if (criteria == "coverageDegree")
            {
                 minValue = criteriaSortedList[0].degree_of_coverage;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].degree_of_coverage;
            }
            else if (criteria == "crossingIntensity")
            {
                 minValue = criteriaSortedList[0].crossing_intensity;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].crossing_intensity;
            }
            else if (criteria == "areaLoss")
            {
                 minValue = criteriaSortedList[0].loss_of_prodArea;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].loss_of_prodArea;
            }
            else if (criteria == "deviation")
            {
                 minValue = criteriaSortedList[0].deviation;
                 maxValue = criteriaSortedList[criteriaSortedList.Count - 1].deviation;
            }


            IDictionary<Mapper, IList<double>> networkMap = new Dictionary<Mapper, IList<double>>();
            IDictionary<int, double> dict = new Dictionary<int, double>();

            dict.Add(1, 0.005);  
            dict.Add(2, 0.010);
            dict.Add(3, 0.015);

            dict.Add(4, 0.020);  
            dict.Add(5, 0.022);
            dict.Add(6, 0.025);

            dict.Add(7, 0.30);  
            dict.Add(8, 0.65);
            dict.Add(9, 1); 


            IList<double> criteriaRangeList = new List<double>();

            if (maxValue - minValue < 10)
            {

                for (var j = minValue; j <= maxValue; j = j + 0.01)
                {
                    criteriaRangeList.Add(j);
                }

            }
            else if (maxValue - minValue < 100)
            {

                for (var j = minValue; j <= maxValue; j = j + 0.1)
                {
                    criteriaRangeList.Add(j);
                }
            }
            else if (maxValue - minValue < 1000)
            {

                for (var j = minValue; j <= maxValue; j = j + 0.2)
                {
                    criteriaRangeList.Add(j);
                }

            }
            else
            {

                for (var j = minValue; j <= maxValue; j = j + 1)
                {
                    criteriaRangeList.Add(j);
                }

            }
     
            var min = (int)minValue;
            var max = (int)maxValue;
            IList<double> networkInformation;
            double mapperLastIndex=0;
            Mapper m;

            for (var i = 1; i < 10; i++)
            {
                var FindBinPercentage = dict[i];
                var limit = 0;
                if (FindBinPercentage == 1)
                {
                    limit = (int)Math.Round((criteriaRangeList.Count * FindBinPercentage)-1);
                }
                else
                {
                    limit = (int)Math.Round((criteriaRangeList.Count * FindBinPercentage));
                }

                networkInformation = new List<double>();
                if (criteriaRangeList.Count > 1)
                {
                    for (var j = 0; j <= limit; j++)
                    {
                        networkInformation.Add(criteriaRangeList[0]);
                        if (criteriaRangeList.Count > 0)
                            criteriaRangeList.Remove(criteriaRangeList[0]);
                    }

                    if (i==1)
                    {
                        m = new Mapper(networkInformation[0], networkInformation[networkInformation.Count - 1]);
                        mapperLastIndex = networkInformation[networkInformation.Count - 1];
                    } else if (i == 9)
                    {
                        m = new Mapper(mapperLastIndex, networkInformation[networkInformation.Count - 1]+1);
                    } else
                    {
                        m = new Mapper(mapperLastIndex, networkInformation[networkInformation.Count - 1]);
                        mapperLastIndex = networkInformation[networkInformation.Count - 1];
                    }

                    networkMap.Add(m, networkInformation);
                    networkInformation.Clear();
                }

            }


            
            IList<double> weights = new List<double>();


            if (criteria == "coverageDegree")
            {
                int binIndex = 0;
                for (var i = 0; i < criteriaList.Count; i++)
                {
                    foreach (KeyValuePair<Mapper, IList<double>> entry in networkMap)
                    {
                        Mapper p = entry.Key;

                        binIndex++;

                        if (criteriaList[i] >= p.StartIndex && criteriaList[i] < p.EndIndex)
                        {
                            weights.Add(binIndex);
                            break;
                        }
                    }
                    binIndex = 0;
                }
            }
            else
            {
                int binIndex = 10;
                for (var i = 0; i < criteriaList.Count; i++)
                {
                    foreach (KeyValuePair<Mapper, IList<double>> entry in networkMap)
                    {
                        Mapper p = entry.Key;

                        binIndex--;

                        if (criteriaList[i] >= p.StartIndex && criteriaList[i] < p.EndIndex)
                        {
                            weights.Add(binIndex);
                            break;
                        }
                    }
                    binIndex = 10;
                }
            }
            

            return weights;
        }
        //Get the priority matrix (a x b) where a - Number of networks, b - Number of criterias
        public Matrix<double> GetPriorityMatrix(int row, int col, List<double> trailLength_priority, List<double>  trailDensity_priority, List<double>  crossCounDist_priority, List<double>  coverageDeg_priority, List<double>  crossingIntensity_priority, List<double> areaLoss_priority, List<double> deviation_priority)
        {

            Matrix<double> priority_matrix = Matrix<double>.Build.Dense(row, col);
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    if (i == 0)
                    {
                        priority_matrix[j, i] = trailLength_priority[j];
                    }
                    else if (i == 1)
                    {
                        priority_matrix[j, i] = trailDensity_priority[j];
                    }
                    else if (i == 2)
                    {
                        priority_matrix[j, i] = crossCounDist_priority[j];
                    }
                    else if (i == 3)
                    {
                        priority_matrix[j, i] = coverageDeg_priority[j];
                    }
                    else if (i == 4)
                    {
                        priority_matrix[j, i] = crossingIntensity_priority[j];
                    }
                    else if (i == 5)
                    {
                        priority_matrix[j, i] = areaLoss_priority[j];
                    }
                    else if (i == 6)
                    {
                        priority_matrix[j, i] = deviation_priority[j];
                    }


                }

            }
            return priority_matrix;
        }

        //Get Priority weighted matrix - Multiply values of each columns with its corressponding prorities
        public Matrix<double> GetPriorityWeightMatrix(Matrix<double> matrix_pw, int row, int col, List<double> criteriaWeights)
        {

            Matrix<double> consistency_weight_matrix = Matrix<double>.Build.Dense(row, col);
            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    consistency_weight_matrix[j, i] = (Convert.ToDouble(matrix_pw[j, i])) * criteriaWeights[i];

                }

            }
            return consistency_weight_matrix;
        }

        //Get final priorities of networks - Add up the priorities of all seven criterias in each row 
        public List<double> GetPriorityWeightedSum(Matrix<double> consistency_weight_matrix, int row, int col)
        {
            consistency_weighted_sum = new List<double>();
            double sum = 0;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sum += (Convert.ToDouble(consistency_weight_matrix[i, j]));

                }
                consistency_weighted_sum.Add(sum);
                sum = 0;
            }

            return consistency_weighted_sum;
        }

        internal class Mapper
        {
            public Mapper(double start, double end)
            {
                StartIndex = start;
                EndIndex = end;
            }
            public double StartIndex { get; }
            public double EndIndex { get; }
        }

    }
}
