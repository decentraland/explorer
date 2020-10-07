import * as path from 'path'
import { Configuration } from 'webpack'
import { TsconfigPathsPlugin } from 'tsconfig-paths-webpack-plugin'

const webpackConfig = (env): Configuration => ({
  entry: './website/src/index.tsx',
  resolve: {
    extensions: ['.ts', '.tsx', '.js'],
    plugins: [new TsconfigPathsPlugin()]
  },
  output: {
    path: path.resolve('./static/dist'),
    filename: 'react.js'
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        loader: 'ts-loader',
        options: {
          transpileOnly: true
        }
        // exclude: /stories/
      },
      {
        test: /\.css$/i,
        use: ['css-loader?url=false']
        // options: {
        //   url: true
        // }
      }
    ]
  },
  devServer: {
    contentBase: path.resolve('../static')
  }
})

export default webpackConfig
