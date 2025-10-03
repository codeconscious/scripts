// Source: https://sci.tea-nifty.com/blog/cat20211520/index.html

#r "nuget: MathNet.Numerics, 5.0.0"
#r "nuget: Plotly.NET"

open MathNet.Numerics.Distributions
open MathNet.Numerics.Random
open Plotly.NET

let rnd = MersenneTwister(42)
let N = 10000
let xmin = -5.0
let xmax = 5.0
let bin = 100
let normal = Array.create N 0.0

Normal.Samples(rnd, normal, 0.0, 1.0)

Chart.Histogram normal
|> Chart.withXAxisStyle("x")
|> Chart.withYAxisStyle("Frequency")
|> Chart.withTitle("Gaussian Distribution")
