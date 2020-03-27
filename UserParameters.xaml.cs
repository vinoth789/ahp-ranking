using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;



namespace LFSmodel
{

    public partial class UserParameters : Page
    {
        
        // AHP **** lists and variables

        public List<string> CriteriaList;
        public List<NetworkEntity>  ntwList;
        public List<double> trailLengthList;
        public List<double> traiLdensityList;
        public List<double> crossCountryDistList;
        public List<double> coverageDegreeList;
        public List<double> crossingIntensityList;
        public List<double> areaLossList;
        public List<double> deviationList;

        Matrix<double>  pairwise_matrix;
        Matrix<double>  trailLengthPairwise_matrix;
        Matrix<double>  trailDensityPairwise_matrix;
        Matrix<double>  crossCountryDistPairwise_matrix;
        Matrix<double>  coverageDegreePairwise_matrix;
        Matrix<double>  crossingIntensityPairwise_matrix;
        Matrix<double>  areaLossPairwise_matrix;
        Matrix<double>  deviationPairwise_matrix;
        AhpProcess ahp = new AhpProcess();
        List<double> column_total = new List<double>();
        List<double> criteriaPriority = new List<double>();
        List<double> consitencyWeightsSum = new List<double>();
        List<double> AlternativesPriorityWeightsSum = new List<double>();
        List<double> ToFindBestNtwList = new List<double>();
        List<int> BestFiveNtwIndices;
        double consistencyRatio = 0;
        NetworkEntity network;

        List<double> trailLength_column_total = new List<double>();
        List<double> trailDensity_column_total = new List<double>();
        List<double> crossCounDist_column_total = new List<double>();
        List<double> coverageDeg_column_total = new List<double>();
        List<double> crossingIntensity_column_total = new List<double>();
        List<double> areaLoss_column_total = new List<double>();
        List<double> deviation_column_total = new List<double>();

        List<double> trailLength_priority = new List<double>();
        List<double> trailDensity_priority = new List<double>();
        List<double> crossCounDist_priority = new List<double>();
        List<double> coverageDeg_priority = new List<double>();
        List<double> crossingIntensity_priority = new List<double>();
        List<double> areaLoss_priority = new List<double>();
        List<double> deviation_priority = new List<double>();

        public UserParameters()
        {
            InitializeComponent();
            this.DataContext = this;

            // AHP **** Initialisations

            CriteriaList = new List<string>();
            CriteriaList.Add("c1");
            CriteriaList.Add("c2");
            CriteriaList.Add("c3");
            CriteriaList.Add("c4");
            CriteriaList.Add("c5");
            CriteriaList.Add("c6");
            CriteriaList.Add("c7");

            ntwList = new List<NetworkEntity>();
            Gasse20.Calc.CalcAlley test = new Gasse20.Calc.CalcAlley();
            Gasse20.Model.AlleyGrid[][][] testnets = test.generate_AlleyGrids();


            for (int i = 0; i < testnets.Length; i++)
            {
                for (int j = 0; j < testnets[i].Length; j++)
                {
                    for (int k = 0; k < testnets[i][j].Length; k++)
                    {

                        network = new NetworkEntity();
                        network.trail_length = testnets[i][j][k].target_criteria.alley_lenght.total;
                        network.trail_density = testnets[i][j][k].target_criteria.alley_density.curent;
                        network.cross_country_transport_dist = testnets[i][j][k].target_criteria.transport_distance.total;
                        network.degree_of_coverage = testnets[i][j][k].target_criteria.Development_area.curent;
                        network.crossing_intensity = testnets[i][j][k].target_criteria.crossing_intensity;
                        network.loss_of_prodArea = testnets[i][j][k].target_criteria.lost_of_production_area.curent;
                        network.deviation = 20+i+j+k;

                        ntwList.Add(network);
                    }
                }
            }

        }

        private void perform_AHP(object sender, EventArgs e)
        {
            double[] weights = { 3, 5, 7, 9, 3, 5, 7, 3, 5, 7, 9, 3, 5, 7, 3, 5, 7, 9, 3, 5, 7 };

            //Pairwise comparisaon
            pairwise_matrix = ahp.PairwiseComparison(CriteriaList.Count, CriteriaList.Count, weights);
            //Column total
            column_total = ahp.SumUpColumns(pairwise_matrix, CriteriaList.Count, CriteriaList.Count);
            //Normalised matrix
            Matrix<double> normalized_matrix = ahp.NormalizeMatrix(pairwise_matrix, CriteriaList.Count, CriteriaList.Count, column_total);
            criteriaPriority = ahp.GetCriteriaWeight(normalized_matrix, CriteriaList.Count, CriteriaList.Count);
            //Consistency matrix
            Matrix<double> consistency_weighted_matrix = ahp.GetConsistencyWeightMatrix(pairwise_matrix, CriteriaList.Count, CriteriaList.Count, criteriaPriority);
            consitencyWeightsSum = ahp.GetConsistencyWeightedSum(consistency_weighted_matrix, CriteriaList.Count, CriteriaList.Count);
            consistencyRatio = ahp.GetConsistencyRatio(consitencyWeightsSum, criteriaPriority, CriteriaList.Count);

            trailLengthList = new List<double>();
            traiLdensityList = new List<double>();
            crossCountryDistList = new List<double>();
            coverageDegreeList = new List<double>();
            crossingIntensityList = new List<double>();
            areaLossList = new List<double>();
            deviationList = new List<double>();

            //Get the criteria values for each network and set it to the NetworkEntity model
            foreach (NetworkEntity network in ntwList)
            {

                trailLengthList.Add(network.trail_length);
                traiLdensityList.Add(network.trail_density);
                crossCountryDistList.Add(network.cross_country_transport_dist);
                coverageDegreeList.Add(network.degree_of_coverage);
                crossingIntensityList.Add(network.crossing_intensity);
                areaLossList.Add(network.loss_of_prodArea);
                deviationList.Add(network.deviation);
            }

            //Sort the values of criteria list - to be used while creating 9 bins
            List<NetworkEntity> trailLengthSortedList = ntwList.OrderBy(o => o.trail_length).ToList();
            List<NetworkEntity> traiLdensitySortedList = ntwList.OrderBy(o => o.trail_density).ToList();
            List<NetworkEntity> crossCountryDistListSortedList = ntwList.OrderBy(o => o.cross_country_transport_dist).ToList();
            List<NetworkEntity> coverageDegreeSortedList = ntwList.OrderBy(o => o.degree_of_coverage).ToList();
            List<NetworkEntity> crossingIntensitySortedList = ntwList.OrderBy(o => o.crossing_intensity).ToList();
            List<NetworkEntity> areaLossSortedList = ntwList.OrderBy(o => o.loss_of_prodArea).ToList();
            List<NetworkEntity> deviationSortedList = ntwList.OrderBy(o => o.deviation).ToList();


            //Build the pairwise matrix for each criterias
            trailLengthPairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);
            trailDensityPairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);
            crossCountryDistPairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);
            coverageDegreePairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);
            crossingIntensityPairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);
            areaLossPairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);
            deviationPairwise_matrix = Matrix<double>.Build.DenseDiagonal(CriteriaList.Count, CriteriaList.Count, 1);

            //Get the weights of each network corresponding to the ´trail length´ criteria
            //Get the pairwise matrix for each network corresponding to the ´trail length´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´trail length´ criteria
            //Get the normalized matrix - network corresponding to the ´trail length´ criteria
            //Get the priority of each network with respect to the ´trail length´ criteria
            IList<double> trailLengthWeights = ahp.Bins(trailLengthList, trailLengthSortedList, "trailLength");
            trailLengthPairwise_matrix = ahp.PairwiseComparisonForAlternatives(trailLengthWeights);
            trailLength_column_total = ahp.SumUpColumns(trailLengthPairwise_matrix, trailLengthWeights.Count, trailLengthWeights.Count);
            Matrix<double> trailLength_normalized_matrix = ahp.NormalizeMatrix(trailLengthPairwise_matrix, trailLengthWeights.Count, trailLengthWeights.Count, trailLength_column_total);
            trailLength_priority = ahp.GetCriteriaWeight(trailLength_normalized_matrix, trailLengthWeights.Count, trailLengthWeights.Count);


            //Get the weights of each network corresponding to the ´trail density´ criteria
            //Get the pairwise matrix for each network corresponding to the ´trail density´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´trail density´ criteria
            //Get the normalized matrix - network corresponding to the ´trail density´ criteria
            //Get the priority of each network with respect to the ´trail density´ criteria
            IList<double> traiLdensityWeights = ahp.Bins(traiLdensityList, traiLdensitySortedList, "trailDensity");
            trailDensityPairwise_matrix = ahp.PairwiseComparisonForAlternatives(traiLdensityWeights);
            trailDensity_column_total = ahp.SumUpColumns(trailDensityPairwise_matrix, traiLdensityWeights.Count, traiLdensityWeights.Count);
            Matrix<double> trailDensity_normalized_matrix = ahp.NormalizeMatrix(trailDensityPairwise_matrix, traiLdensityWeights.Count, traiLdensityWeights.Count, trailDensity_column_total);
            trailDensity_priority = ahp.GetCriteriaWeight(trailDensity_normalized_matrix, traiLdensityWeights.Count, traiLdensityWeights.Count);


            //Get the weights of each network corresponding to the ´cross country distance´ criteria
            //Get the pairwise matrix for each network corresponding to the ´cross country distance´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´cross country distance´ criteria
            //Get the normalized matrix - network corresponding to the ´cross country distance´ criteria
            //Get the priority of each network with respect to the ´cross country distance´ criteria
            IList<double> crossCountryDistWeights = ahp.Bins(crossCountryDistList, crossCountryDistListSortedList, "crossCountryDist");
            crossCountryDistPairwise_matrix = ahp.PairwiseComparisonForAlternatives(crossCountryDistWeights);
            crossCounDist_column_total = ahp.SumUpColumns(crossCountryDistPairwise_matrix, crossCountryDistWeights.Count, crossCountryDistWeights.Count);
            Matrix<double> crossCounDist_normalized_matrix = ahp.NormalizeMatrix(crossCountryDistPairwise_matrix, crossCountryDistWeights.Count, crossCountryDistWeights.Count, crossCounDist_column_total);
            crossCounDist_priority = ahp.GetCriteriaWeight(crossCounDist_normalized_matrix, crossCountryDistWeights.Count, crossCountryDistWeights.Count);


            //Get the weights of each network corresponding to the ´degree of coverage´ criteria
            //Get the pairwise matrix for each network corresponding to the ´degree of coverage´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´degree of coverage´ criteria
            //Get the normalized matrix - network corresponding to the ´degree of coverage´ criteria
            //Get the priority of each network with respect to the ´degree of coverage´ criteria
            IList<double> coverageDegreeWeights = ahp.Bins(coverageDegreeList, coverageDegreeSortedList, "coverageDegree");
            coverageDegreePairwise_matrix = ahp.PairwiseComparisonForAlternatives(coverageDegreeWeights);
            coverageDeg_column_total = ahp.SumUpColumns(coverageDegreePairwise_matrix, coverageDegreeWeights.Count, coverageDegreeWeights.Count);
            Matrix<double> coverageDeg_normalized_matrix = ahp.NormalizeMatrix(coverageDegreePairwise_matrix, coverageDegreeWeights.Count, coverageDegreeWeights.Count, coverageDeg_column_total);
            coverageDeg_priority = ahp.GetCriteriaWeight(coverageDeg_normalized_matrix, coverageDegreeWeights.Count, coverageDegreeWeights.Count);


            //Get the weights of each network corresponding to the ´crossing intensity´ criteria
            //Get the pairwise matrix for each network corresponding to the ´crossing intensity´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´crossing intensity´ criteria
            //Get the normalized matrix - network corresponding to the ´crossing intensity´ criteria
            //Get the priority of each network with respect to the ´crossing intensity´ criteria
            IList<double> crossingIntensityWeights = ahp.Bins(crossingIntensityList, crossingIntensitySortedList, "crossingIntensity");
            crossingIntensityPairwise_matrix = ahp.PairwiseComparisonForAlternatives(crossingIntensityWeights);
            crossingIntensity_column_total = ahp.SumUpColumns(crossingIntensityPairwise_matrix, crossingIntensityWeights.Count, crossingIntensityWeights.Count);
            Matrix<double> crossingIntensity_normalized_matrix = ahp.NormalizeMatrix(crossingIntensityPairwise_matrix, crossingIntensityWeights.Count, crossingIntensityWeights.Count, crossingIntensity_column_total);
            crossingIntensity_priority = ahp.GetCriteriaWeight(crossingIntensity_normalized_matrix, crossingIntensityWeights.Count, crossingIntensityWeights.Count);


            //Get the weights of each network corresponding to the ´loss of area´ criteria
            //Get the pairwise matrix for each network corresponding to the ´loss of area´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´loss of area´ criteria
            //Get the normalized matrix - network corresponding to the ´loss of area´ criteria
            //Get the priority of each network with respect to the ´loss of area´ criteria
            IList<double> areaLossWeights = ahp.Bins(areaLossList, areaLossSortedList, "areaLoss");
            areaLossPairwise_matrix = ahp.PairwiseComparisonForAlternatives(areaLossWeights);
            areaLoss_column_total = ahp.SumUpColumns(areaLossPairwise_matrix, areaLossWeights.Count, areaLossWeights.Count);
            Matrix<double> areaLoss_normalized_matrix = ahp.NormalizeMatrix(areaLossPairwise_matrix, areaLossWeights.Count, areaLossWeights.Count, areaLoss_column_total);
            areaLoss_priority = ahp.GetCriteriaWeight(areaLoss_normalized_matrix, areaLossWeights.Count, areaLossWeights.Count);


            //Get the weights of each network corresponding to the ´deviation´ criteria
            //Get the pairwise matrix for each network corresponding to the ´deviation´ criteria
            //Get the column total from pairwise matrix - network corresponding to the ´deviation´ criteria
            //Get the normalized matrix - network corresponding to the ´deviation´ criteria
            //Get the priority of each network with respect to the ´deviation´ criteria
            IList<double> deviationWeights = ahp.Bins(deviationList, deviationSortedList, "deviation");
            deviationPairwise_matrix = ahp.PairwiseComparisonForAlternatives(deviationWeights);
            deviation_column_total = ahp.SumUpColumns(deviationPairwise_matrix, deviationWeights.Count, deviationWeights.Count);
            Matrix<double> deviation_normalized_matrix = ahp.NormalizeMatrix(deviationPairwise_matrix, deviationWeights.Count, deviationWeights.Count, deviation_column_total);
            deviation_priority = ahp.GetCriteriaWeight(deviation_normalized_matrix, deviationWeights.Count, deviationWeights.Count);

            //Local Priorities(or preferences) of the networks with respect to each criterion
            Matrix<double> priority_matrix = ahp.GetPriorityMatrix(trailLengthWeights.Count, 7, trailLength_priority, trailDensity_priority, crossCounDist_priority, coverageDeg_priority, crossingIntensity_priority, areaLoss_priority, deviation_priority);
            //Get priority weighted matrix - multiply each criterion priorities with the values of each column
            Matrix<double> Alternatives_priority_weighted_matrix = ahp.GetPriorityWeightMatrix(priority_matrix, trailLengthWeights.Count, 7, criteriaPriority);
            //Get final network priority list - Addup values of each column row wise
            AlternativesPriorityWeightsSum = ahp.GetPriorityWeightedSum(Alternatives_priority_weighted_matrix, trailLengthWeights.Count, 7);
            List<double> networkRanking = AlternativesPriorityWeightsSum.OrderBy(o => o).ToList();
            ToFindBestNtwList = AlternativesPriorityWeightsSum;

            //Get the best five networks out of all the networks
            double max;
            int index;
            BestFiveNtwIndices = new List<int>();
            IDictionary<int, double> bestNetworks = new Dictionary<int, double>();
            for (int j = 0; j < 5; j++)
            {
                max = ToFindBestNtwList[0];
                index = 0;
                for (int i = 0; i < ToFindBestNtwList.Count; i++)
                {
                    if (max < ToFindBestNtwList[i])
                    {
                        max = ToFindBestNtwList[i];
                        index = i;
                    }
                }
                BestFiveNtwIndices.Add(index);
                bestNetworks.Add(index, max);
                ToFindBestNtwList[index] = 0;

            }

        }
     
    } 

}
